public static class MathEx
{
    public static UnityEngine.Vector3 deleteZ(UnityEngine.Vector3 vector3) {return new UnityEngine.Vector3(vector3.x,vector3.y,0f);}

    public static int abs(int a){return a < 0 ? -a : a;}
    public static float abs(float a){return a < 0 ? -a : a;}
    


    public static float lerpf(float a, float b, float time){return a + (b - a) * time;}
    public static UnityEngine.Vector2 lerpV2(UnityEngine.Vector2 a, UnityEngine.Vector2 b, float time) {return new UnityEngine.Vector2(lerpf(a.x,b.x,time),lerpf(a.y,b.y,time));}
    public static UnityEngine.Vector3 lerpV3WithoutZ(UnityEngine.Vector3 a, UnityEngine.Vector3 b, float time) {return new UnityEngine.Vector3(lerpf(a.x,b.x,time),lerpf(a.y,b.y,time));}
    public static int mini(int a, int b){return a < b ? a : b;}
    public static int maxi(int a, int b){return a > b ? a : b;}
    public static float minf(float a, float b){return a < b ? a : b;}
    public static float maxf(float a, float b){return a > b ? a : b;}
    public static int clampi(int value, int a, int b){return mini(maxi(value,a), b);}
    public static float clampf(float value, float a, float b){return minf(maxf(value,a), b);}
    public static float clamp01f(float value){return value < 0f ? 0f : (value > 1f ? 1f : value);}
    public static float clampDegree(float degree)
    {
        while(degree < 0f) {degree += 360f;}
        while(degree > 360f) {degree -= 360f;}
        
        return degree;
    }

	public static UnityEngine.Vector3 convergence0(UnityEngine.Vector3 value, float a)
	{
		if(MathEx.equals(a,0f,float.Epsilon))
			return value;
			
		if(value.sqrMagnitude < float.Epsilon)
			return UnityEngine.Vector3.zero;
			
		UnityEngine.Vector3 vector = value.normalized * a;
		float result = value.sqrMagnitude - vector.sqrMagnitude;

		return result <= 0f ? UnityEngine.Vector3.zero : value - vector;
	}

	public static UnityEngine.Vector3 convergenceTarget(UnityEngine.Vector3 value, UnityEngine.Vector3 target, float a)
	{
		if(MathEx.equals(a,0f,float.Epsilon))
			return value;
			
		UnityEngine.Vector3 direction = (target - value).normalized;

        UnityEngine.Vector3 result = value + (direction * a);
		UnityEngine.Vector3 resultDirection = (target - result).normalized;

		return UnityEngine.Vector3.Angle(direction,resultDirection) > 100f ? target : result;
	}

    public static float convergence0(float value, float a)
    {
        if(equals(value,0f,float.Epsilon))
            return 0f;
            
        float mark = normalize(value);
        float result = abs(value) - abs(a);

        return result <= 0f ? 0f : result * mark;
    }

    public static float convergenceTarget(float value, float target, float a)
    {
        if(equals(value,target,float.Epsilon))
            return target;
            
		float mark = target > value ? 1f : -1f;
        float result = value + (abs(a) * mark);

        return (abs(target) - abs(result)) * mark <= 0f ? target : result;
    }

    public static float normalize(float value) {return value < 0f ? -1f : 1f;}

    public static bool equals(int a, int b){return a==b;}
    public static bool equals(float a, float b, float epsilon){return abs(a-b) < epsilon;}
    public static bool equals(UnityEngine.Color a, UnityEngine.Color b, float epsilon)
    {
        return equals(a.r,b.r,epsilon) && equals(a.g,b.g,epsilon) && equals(a.b,b.b,epsilon) && equals(a.a,b.a,epsilon);
    }


	public static float directionToAngle(UnityEngine.Vector2 dir) 
	{
		float val = UnityEngine.Mathf.Atan2(dir.y, dir.x) * UnityEngine.Mathf.Rad2Deg;

		return clamp360Degree(val);
	}
	public static UnityEngine.Vector3 angleToDirection(float angle) {return new UnityEngine.Vector3(UnityEngine.Mathf.Cos(angle),UnityEngine.Mathf.Sin(angle));}
	public static float clamp360Degree(float eulerAngle)
    {
        //  float val = eulerAngle - Mathf.CeilToInt(eulerAngle / 360f) * 360f;
		//  val = val < 0 ? val + 360f : val;
		float val = eulerAngle + ((float)((int)-eulerAngle / 360) * 360f);
		val = val < 0 ? val + 360f : val;
		return val;
    }


    public static void makeTriangle(UnityEngine.Vector3 centerPosition,float radius, float theta, float directionAngle, out UnityEngine.Vector3 triangleA, out UnityEngine.Vector3 triangleB, out UnityEngine.Vector3 triangleC)
    {
        UnityEngine.Vector3 directionVector = UnityEngine.Quaternion.Euler(0f,0f,directionAngle) * UnityEngine.Vector3.right * radius;
        
        triangleA = centerPosition;
        triangleB = triangleA + UnityEngine.Quaternion.Euler(0f,0f,theta * .5f) * directionVector;
        triangleC = triangleA + UnityEngine.Quaternion.Euler(0f,0f,-theta * .5f) * directionVector;
    }

    public static bool isInTriangle(UnityEngine.Vector3 target, UnityEngine.Vector3 a, UnityEngine.Vector3 b, UnityEngine.Vector3 c, out float t, out float s, out float ots)
	{
		getBarycentricWeight(target, a, b, c, out t, out s, out ots);

		return t >= 0 && t <= 1 && s >= 0 && s <= 1 && ots >= 0 && ots <= 1;
	}

	public static void getBarycentricWeight(UnityEngine.Vector3 target, UnityEngine.Vector3 a, UnityEngine.Vector3 b, UnityEngine.Vector3 c, out float t, out float s, out float ots)
	{
		UnityEngine.Vector3 u = c - b;
		UnityEngine.Vector3 v = a - b;

		UnityEngine.Vector3 w = target - b;

		float uu = UnityEngine.Vector3.Dot(u, u);
		float vv = UnityEngine.Vector3.Dot(v, v);
		float uv = UnityEngine.Vector3.Dot(u, v);
		float wu = UnityEngine.Vector3.Dot(w, u);
		float wv = UnityEngine.Vector3.Dot(w, v);

		float denominator = uv * uv - uu * vv;
		t = (wu * uv - wv * uu) / denominator;
		s = (wv * uv - wu * vv) / denominator;
		ots = 1f - s - t;
	}

	public static bool pointSphereRayCast(UnityEngine.Vector3 currentPosition,UnityEngine.Vector3 targetPosition, UnityEngine.Vector3 direction, float radius)
	{
		UnityEngine.Vector3 perpendicularPoint = getPerpendicularPointOnLineSegment(currentPosition,currentPosition + direction,targetPosition);
		return UnityEngine.Vector3.Distance(perpendicularPoint, targetPosition) < radius;
	}

	public static UnityEngine.Vector3 getPerpendicularPointOnLineSegment(UnityEngine.Vector3 lineA, UnityEngine.Vector3 lineB, UnityEngine.Vector3 point)
	{
		UnityEngine.Vector3 pa = point - lineA;
		UnityEngine.Vector3 ab = lineB - lineA;

		float len = ab.sqrMagnitude;
		float dot = UnityEngine.Vector3.Dot(ab, pa);
		float distance = dot / len;

		if (distance < 0)
			return lineA;
		else if (distance > 1)
			return lineB;
		else
			return lineA + ab * distance;

	}

	public static bool findNearestPointOnTriangle(UnityEngine.Vector3 target, UnityEngine.Vector3 a, UnityEngine.Vector3 b, UnityEngine.Vector3 c, out UnityEngine.Vector3 result, out float nearDistance )
	{
        float t = 0f;
        float s = 0f;
        float ots = 0f;

        nearDistance = 0f;

        result = UnityEngine.Vector3.zero;

		if (isInTriangle(target, a, b, c, out t, out s, out ots))
			return false;

		result = target;
		nearDistance = float.MaxValue;
		if (s < 0)
		{
			result = getPerpendicularPointOnLineSegment(a, b, target);

			nearDistance = UnityEngine.Vector3.Distance(result, target);
		}

		if (t < 0)
		{
			UnityEngine.Vector3 point = getPerpendicularPointOnLineSegment(b, c, target);

			float distance = UnityEngine.Vector3.Distance(point, target);

			if (nearDistance > distance)
			{
				nearDistance = distance;
				result = point;
			}
		}

		if (ots < 0)
		{
			UnityEngine.Vector3 point = getPerpendicularPointOnLineSegment(c, a, target);

			float distance = UnityEngine.Vector3.Distance(point, target);

			if (nearDistance > distance)
			{
				nearDistance = distance;
				result = point;
			}
		}

		getBarycentricWeight(result, a, b, c, out t, out s, out ots);
		return true;
	}
}