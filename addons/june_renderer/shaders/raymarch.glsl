#[compute]
#version 450

const int maxSteps = 100;
const float minDistance = 0.0001;
const float maxDistance = 1000.;

const float tanFov = 0.767326987979;

const vec3 light = normalize(vec3(1.0, 1.0, -1.0));
const float ambient = 0.2;

const int dataBufferObjectSize = 4;



// Invocations in the (x, y, z) dimension
layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;

//Image uniforms
layout(rgba8, set = 0, binding = 0) uniform image2D depth_image;

//Input object types
layout(set = 0, binding = 1, std430) restrict buffer ObjectTypes { int data[]; } object_types;

//Input object data (positions, radius, etc.)
layout(set = 0, binding = 2, std430) restrict buffer ObjectPositions { float data[]; } object_positions;

//Globally accessed by conemarch function and main function
struct HitResult
{
	bool hit;
	vec3 pos;
    float dist;
	int steps;
} hitResult;


float sdSphere( vec3 testPoint, vec3 objectPos, float radius )
{
  	return length(testPoint - objectPos) - radius;
}

float sdBox( vec3 testPoint, vec3 objectPos, vec3 halfSize )
{
	testPoint -= objectPos;
	vec3 q = abs(testPoint) - halfSize;
	return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}



float smin(float a, float b, float smoothing) {
	float h = clamp(0.5 + 0.5 * (b - a) / smoothing, 0.0, 1.0);
	return mix(b, a, h) - smoothing * h * (1.0 - h);
}

vec3 opRep(vec3 testPoint, vec3 c) {
    vec3 q = mod(testPoint+0.5*c,c)-0.5*c;
    return q;
}




vec2 intersectAABB(vec3 rayOrigin, vec3 rayDir, vec3 boxPosition, vec3 boxHalfSize) {
	vec3 boxMin = boxPosition - boxHalfSize;
	vec3 boxMax = boxPosition + boxHalfSize;

    vec3 tMin = (boxMin - rayOrigin) / rayDir;
    vec3 tMax = (boxMax - rayOrigin) / rayDir;
    vec3 t1 = min(tMin, tMax);
    vec3 t2 = max(tMin, tMax);
    float tNear = max(max(t1.x, t1.y), t1.z);
    float tFar = min(min(t2.x, t2.y), t2.z);
    return vec2(tNear, tFar);
}

float SDF(vec3 point)
{
	float closest = 9999999999.;
	for (int i = 0; i < object_types.data.length(); i++)
	{
		//Continue if ray doesn't intersect AABB
		//if (object_types.data[i] == -1) continue;

		int offset = dataBufferObjectSize * i;
		vec3 objectPos = vec3(object_positions.data[offset + 0], object_positions.data[offset + 1], object_positions.data[offset + 2]);
		float dist;

		if (object_types.data[i] == 0) dist = sdSphere(point, objectPos, object_positions.data[offset + 3]);
		//else if (object_types.data[i] == 1) dist = sdBox(point, objectPos, vec3(object_positions.data[offset + 3]));

		closest = min(closest, dist);
	}
	return closest;
}

vec3 calcNormal( in vec3 p ) // for function f(p)
{
    const float eps = 0.0001; // or some other value
    const vec2 h = vec2(eps,0);
    return normalize( vec3(SDF(p+h.xyy) - SDF(p-h.xyy),
                           SDF(p+h.yxy) - SDF(p-h.yxy),
                           SDF(p+h.yyx) - SDF(p-h.yyx) ) );
}



void raymarch(vec3 pos, vec3 dir, float startingDepth, int subdivisions)
{
	hitResult.hit = false;
	hitResult.steps = 0;
	hitResult.pos = pos;
    hitResult.dist = startingDepth;

	for (hitResult.steps = 0; hitResult.steps < maxSteps; hitResult.steps++)
	{
		float marchDistance = SDF(hitResult.pos);

		//Mysterious value is the square root of 2, aka the distance from one corner of a unit square to the opposite
		//Multiplied to make sure circular cone takes up the entire square
		if (marchDistance < minDistance)
		{
			hitResult.hit = true;
			break;
		}
		if (marchDistance > maxDistance) break;

		hitResult.pos += dir * marchDistance;
        hitResult.dist += marchDistance;
	}
}




void main() {
	//gl_GlobalInvocationID is in pixel coorinates so we need to divide by the image resolution
    ivec2 pixel = ivec2(gl_GlobalInvocationID.xy);
	ivec2 size = imageSize(depth_image);
	vec2 uv = vec2(pixel) / size;

	if (uv.x >= 1.0 || uv.y >= 1.0) {
		return;
	}

	//Move UV to center
	uv += vec2(1.) / (size * 2.);
	uv -= vec2(0.5);
	uv *= 2.;
	
	//Correct uv for aspect ratio
	uv.x *= (float(size.x) / size.y);

    //FOV is already the tangent of fov/2 so we don't need to calculate on each thread
	uv *= tanFov;

	vec3 pos = vec3(0., 0., 5.);
	vec3 dir = normalize(vec3(uv.x, -uv.y, -1.0));

	//Ray-AABB check to shift forward cheaply
	// if (size.x == 512)
	// {
	// 	float minDist = 999999999999999.;
	// 	bool hit = false;
	// 	for (int i = 0; i < object_types.data.length(); i++)
	// 	{
	// 		int offset = dataBufferObjectSize * i;
	// 		vec3 objectPos = vec3(object_positions.data[offset + 0], object_positions.data[offset + 1], object_positions.data[offset + 2]);
	// 		vec2 intersect = intersectAABB(pos, dir, objectPos, vec3(1.0));
	// 		if (intersect.x <= intersect.y) 
	// 		{
	// 			minDist = min(minDist, intersect.x);
	// 			hit = true;
	// 		}
	// 	}
	// 	startingDepth = minDist * float(hit);
	// }
	vec2 intersection = intersectAABB(pos, dir, vec3(0.), vec3(1.0));
	if (intersection.x > intersection.y) return;

	raymarch(pos, dir, 0.0, size.x);

    //Use first one in the future for deferred-ish (rgb are normal, alpha is depth)
	//if (params.finalPass) imageStore(output_image, pixel, float(hitResult.hit) * vec4(calcNormal(hitResult.pos), distance(hitResult.pos, pos)));
	imageStore(depth_image, pixel, vec4(vec3(0.0), float(hitResult.hit) * hitResult.dist));
}