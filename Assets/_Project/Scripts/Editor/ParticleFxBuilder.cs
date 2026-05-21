#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ChemistryGame.EditorTools
{
    /// <summary>
    /// Procedurally generate chemistry-specific particle prefabs:
    ///   • Evaporation smoke (burner)
    ///   • Precipitate fall (crystallize)
    ///   • Gas bubbles rising (gas evolve)
    ///   • Reaction sparkle (success)
    ///   • Pour droplet (drop substance)
    /// Lưu vào Assets/_Project/Prefabs/FX/.
    /// </summary>
    public static class ParticleFxBuilder
    {
        private const string DIR = "Assets/_Project/Prefabs/FX";

        [MenuItem("ChemistryGame/Build/Particle FX")]
        public static void BuildAll()
        {
            CreateEvaporation();
            CreatePrecipitate();
            CreateGasBubble();
            CreateSparkleBurst();
            CreatePourDroplet();
            AssetDatabase.SaveAssets();
            Debug.Log("[ParticleFxBuilder] All chemistry particles built.");
        }

        private static GameObject Save(GameObject go, string path)
        {
            var p = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return p;
        }

        private static ParticleSystem ConfigBasic(GameObject go, Color color, float life, float speed,
            float rate, ParticleSystemSimulationSpace space = ParticleSystemSimulationSpace.World)
        {
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = color;
            main.startLifetime = life;
            main.startSpeed = speed;
            main.startSize = 0.25f;
            main.maxParticles = 200;
            main.simulationSpace = space;
            main.loop = false;
            main.duration = 1.5f;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.2f;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            return ps;
        }

        public static GameObject CreateEvaporation()
        {
            var go = new GameObject("FX_Evaporation");
            // Main: rising white smoke
            var ps = ConfigBasic(go, new Color(0.95f, 0.95f, 1f, 0.6f), 2.2f, 1.5f, 18f);
            var main = ps.main; main.startSize = 0.35f; main.duration = 1.8f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f; shape.radius = 0.15f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]{ new GradientColorKey(new Color(1, 1, 1), 0f), new GradientColorKey(new Color(0.9f, 0.9f, 0.95f), 1f) },
                new[]{ new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0.4f, 0.5f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var sol = ps.sizeOverLifetime; sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.6f, 1, 1.6f));

            // Sub-emitter: small sparkle showing crystallization
            var sparkleChild = new GameObject("Sparkle");
            sparkleChild.transform.SetParent(go.transform, false);
            var sps = ConfigBasic(sparkleChild, new Color(1f, 0.95f, 0.5f, 1f), 0.8f, 0.5f, 30f);
            var sMain = sps.main; sMain.startSize = 0.08f; sMain.duration = 1.5f;
            var sShape = sps.shape;
            sShape.shapeType = ParticleSystemShapeType.Sphere; sShape.radius = 0.3f;
            return Save(go, $"{DIR}/FX_Evaporation.prefab");
        }

        public static GameObject CreatePrecipitate()
        {
            var go = new GameObject("FX_Precipitate");
            var ps = ConfigBasic(go, new Color(0.95f, 0.95f, 0.95f, 0.9f), 1.6f, 0.4f, 40f);
            var main = ps.main;
            main.startSize = 0.12f; main.duration = 1f;
            main.gravityModifier = 1.8f; // falling
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.6f, 0.05f, 0f);
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]{ new GradientColorKey(new Color(1, 1, 1), 0f), new GradientColorKey(new Color(0.9f, 0.9f, 0.95f), 1f) },
                new[]{ new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            return Save(go, $"{DIR}/FX_Precipitate.prefab");
        }

        public static GameObject CreateGasBubble()
        {
            var go = new GameObject("FX_GasBubble");
            var ps = ConfigBasic(go, new Color(0.9f, 1f, 0.95f, 0.55f), 1.5f, 0.8f, 35f);
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
            main.duration = 1.2f;
            main.gravityModifier = -0.3f; // float up
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;
            var sol = ps.sizeOverLifetime; sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 0.5f, 1, 1.2f));
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]{ new GradientColorKey(new Color(0.9f, 1, 0.95f), 0f), new GradientColorKey(new Color(0.85f, 0.95f, 1f), 1f) },
                new[]{ new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0.5f, 0.7f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            return Save(go, $"{DIR}/FX_GasBubble.prefab");
        }

        public static GameObject CreateSparkleBurst()
        {
            var go = new GameObject("FX_SparkleBurst");
            var ps = ConfigBasic(go, new Color(1f, 0.95f, 0.5f, 1f), 1.2f, 2f, 0f);
            var main = ps.main;
            main.startSize = 0.18f;
            main.duration = 0.6f;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 40) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            var col = ps.colorOverLifetime; col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[]{ new GradientColorKey(new Color(1, 0.95f, 0.5f), 0f), new GradientColorKey(new Color(1, 0.7f, 0.3f), 1f) },
                new[]{ new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            return Save(go, $"{DIR}/FX_SparkleBurst.prefab");
        }

        public static GameObject CreatePourDroplet()
        {
            var go = new GameObject("FX_PourDroplet");
            var ps = ConfigBasic(go, new Color(0.5f, 0.85f, 1f, 0.85f), 0.8f, 0.6f, 50f);
            var main = ps.main;
            main.startSize = 0.1f;
            main.duration = 0.7f;
            main.gravityModifier = 1.2f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f; shape.radius = 0.1f;
            return Save(go, $"{DIR}/FX_PourDroplet.prefab");
        }
    }
}
#endif
