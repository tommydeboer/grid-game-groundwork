using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace GridGame
{
    public class Lamp : MonoBehaviour
    {
        [Header("Light Settings")]
        [SerializeField]
        Color lightColor = new(255, 210, 155);

        [SerializeField, Range(0f, 5f)]
        float lightIntensity = 1f;

        [SerializeField]
        bool lightShadows = true;

        [Header("Glow Settings")]
        [SerializeField, ColorUsage(true, true)]
        Color glowColor = new(2.67f, 1.09f, 0.04f, 1);

        [Header("References")]
        [SerializeField]
        Light lampLight;

        [SerializeField]
        GameObject lampObject;

        [SerializeField]
        Material defaultMaterial;

        Color currentLightColor;
        float currentLightIntensity;
        bool currentLightShadows;
        Color currentGlowColor;
        static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");

        void Awake()
        {
            SetLightColor(lightColor);
            SetLightIntensity(lightIntensity);
            SetLightShadows(lightShadows);
            SetGlowColor(glowColor);
        }

        void OnValidate()
        {
            if (lightColor != currentLightColor)
            {
                SetLightColor(lightColor);
                currentLightColor = lightColor;
            }

            if (lightIntensity != currentLightIntensity)
            {
                SetLightIntensity(lightIntensity);
                currentLightIntensity = lightIntensity;
            }

            if (lightShadows != currentLightShadows)
            {
                SetLightShadows(lightShadows);
                currentLightShadows = lightShadows;
            }

            if (glowColor != currentGlowColor)
            {
                SetGlowColor(glowColor);
                currentGlowColor = glowColor;
            }
        }

        void SetLightColor(Color color)
        {
            lampLight.color = color;
        }

        void SetLightIntensity(float intensity)
        {
            lampLight.intensity = intensity;
        }

        void SetLightShadows(bool shadows)
        {
            lampLight.shadows = shadows ? LightShadows.Hard : LightShadows.None;
        }

        void SetGlowColor(Color color)
        {
            var lampRenderer = lampObject.GetComponent<Renderer>();
            var tempMaterial = new Material(defaultMaterial);
            tempMaterial.EnableKeyword("_EMISSION");
            tempMaterial.SetColor(emissionColor, color);
            lampRenderer.material = tempMaterial;
        }
    }
}