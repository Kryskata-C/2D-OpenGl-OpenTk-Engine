#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D texture1;

// -- Player / Light uniforms --
uniform vec2  u_PlayerPos;     // Player position in NDC ([-1..1, -1..1])
uniform vec2  u_FlashlightDir; // Normalized direction vector in NDC
uniform float u_LightRadius;   // Distance of flashlight
uniform float u_ConeAngle;     // Half-angle (in radians) of the cone
uniform vec3  u_LightColor;    
uniform vec2  u_WindowSize;    // For converting gl_FragCoord to NDC

void main()
{
    // 1) Get the base color
    vec4 baseColor = texture(texture1, TexCoord);

    // 2) Convert this fragment's position from gl_FragCoord to NDC
    float ndcX = (gl_FragCoord.x / u_WindowSize.x) * 2.0 - 1.0;
    float ndcY = (gl_FragCoord.y / u_WindowSize.y) * 2.0 - 1.0;


    vec2 fragPosNDC = vec2(ndcX, ndcY);

    // 3) Vector from player to this fragment
    vec2 toFrag = fragPosNDC - u_PlayerPos;

    // 4) Distance-based falloff
    float dist = length(toFrag);
    float distanceFactor = 1.0 - (dist / u_LightRadius);
    distanceFactor = clamp(distanceFactor, 0.0, 1.0);

    // 5) Cone check
    // Normalize toFrag, dot it with flashlight direction
    // If dot < cos(u_ConeAngle), then we are outside the cone
    vec2 toFragDir = normalize(toFrag);
    float d = dot(toFragDir, normalize(u_FlashlightDir));
    float coneCutoff = cos(u_ConeAngle);
    
    // If the angle is too large, kill the light factor
    float coneFactor = 0.0;
    if (d > coneCutoff)
    {
        coneFactor = 1.0; // inside the cone
    }

    // 6) Combine the two factors
    float intensity = distanceFactor * coneFactor;

    // 7) Final lighting
    vec3 finalColor = baseColor.rgb * (u_LightColor * intensity);
    FragColor = vec4(finalColor, baseColor.a);
}
