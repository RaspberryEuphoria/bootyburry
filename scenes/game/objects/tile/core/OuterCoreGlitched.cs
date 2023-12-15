using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Game
{
  public partial class OuterCoreGlitched : Sprite2D
  {
    private IEnumerable<CpuParticles2D> particles = System.Array.Empty<CpuParticles2D>();

    public void Init(bool hasTopParticles, bool hasRightParticles, bool hasBottomParticles, bool hasLeftParticles)
    {
      var particlesNodes = GetParticles();
      var hasParticlesFlags = new[] { hasTopParticles, hasRightParticles, hasBottomParticles, hasLeftParticles };

      for (int i = 0; i < particlesNodes.Length; i++)
      {
        if (hasParticlesFlags[i])
        {
          particles = particles.Append(particlesNodes[i]);
        }
        else
        {
          particlesNodes[i].QueueFree();
        }
      }
    }

    public void EmitWaves()
    {
      foreach (var particle in particles)
      {
        particle.Emitting = true;
      }
    }

    private CpuParticles2D[] GetParticles()
    {
      return GetNode("Particles").GetChildren().OfType<CpuParticles2D>().ToArray();
    }
  }
}