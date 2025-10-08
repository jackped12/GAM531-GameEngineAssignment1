#version 330 core
in vec3 FragPos;
in vec3 Normal;

out vec4 FragColor;

// Camera
uniform vec3 viewPos;

// Point light
uniform vec3 lightPos;
uniform vec3 lightColor;
uniform float lightIntensity;

// Material
uniform vec3 matAmbient;
uniform vec3 matDiffuse;
uniform vec3 matSpecular;
uniform float matShininess;

void main()
{
    // Normalize inputs
    vec3 N = normalize(Normal);
    vec3 L = normalize(lightPos - FragPos);
    vec3 V = normalize(viewPos - FragPos);

    // Ambient
    vec3 ambient = matAmbient * lightColor * lightIntensity;

    // Diffuse
    float diff = max(dot(N, L), 0.0);
    vec3 diffuse = matDiffuse * diff * lightColor * lightIntensity;

    // Specular (Blinn-Phong)
    vec3 H = normalize(L + V);
    float spec = pow(max(dot(N, H), 0.0), matShininess);
    vec3 specular = matSpecular * spec * lightColor * lightIntensity;

    FragColor = vec4(ambient + diffuse + specular, 1.0);
}
