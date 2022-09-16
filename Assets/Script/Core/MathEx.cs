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

    public static float convergence0(float value, float a)
    {
        if(equals(value,0f,float.Epsilon))
            return 0f;
            
        float mark = normalize(value);
        float result = abs(value) - abs(a);

        return result <= 0f ? 0f : result * mark;
    }

    public static float normalize(float value) {return value < 0f ? -1f : 1f;}

    public static bool equals(int a, int b){return a==b;}
    public static bool equals(float a, float b, float epsilon){return abs(a-b) < epsilon;}
    public static bool equals(UnityEngine.Color a, UnityEngine.Color b, float epsilon)
    {
        return equals(a.r,b.r,epsilon) && equals(a.g,b.g,epsilon) && equals(a.b,b.b,epsilon) && equals(a.a,b.a,epsilon);
    }
}