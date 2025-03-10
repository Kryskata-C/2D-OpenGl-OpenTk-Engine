#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D texture1;

void main()
{
    FragColor = texture(texture1, TexCoord);
    if (FragColor.a < 0.1) discard; // Remove fully transparent pixels
}
