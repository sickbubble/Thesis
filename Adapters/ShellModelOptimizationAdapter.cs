using OptimizationAlgorithms.Particles;
using OptimizationAlgorithms.Swarms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapters
{

    public interface IParticleSwarmOptimizationAdaptee : IOptimizationAdaptee<ISwarm>
    {
        double[] GetDisplacementProfile();
        double[] GetBestSolution();
    }


    // Adapter class
    public class RunDataOptimizationAdapter : IAdapter
    {

        public RunDataOptimizationAdapter(IParticleSwarmOptimizationAdaptee adaptee)
        {
            Adaptee = adaptee;
        }


        private IAdaptee _RunData;

        public IAdaptee Adaptee { get => _RunData; private set => _RunData = value; }

        public double[] GetBestSolution() 
        {
            return (Adaptee as IParticleSwarmOptimizationAdaptee).GetBestSolution();
        }

        public double[] GetDisplacementProfile()
        {
            return (Adaptee as IParticleSwarmOptimizationAdaptee).GetDisplacementProfile();
        }


        public ISwarm GetOptimizationParticle()
        {
            return (_RunData as IParticleSwarmOptimizationAdaptee).GetOptimizationObject();
        }
       
    }


    // Adapter class
    public class LatticeModelOptimizationAdapter : IOptimizationAdapter<IParticle>
    {

        public LatticeModelOptimizationAdapter(IOptimizationAdaptee<ISwarm> adaptee)
        {
            Adaptee = adaptee;
        }


        private IAdaptee _LatticeModelResultData;

        public IAdaptee Adaptee { get => _LatticeModelResultData; private set => _LatticeModelResultData = value; }


        public ISwarm GetOptimizationOnject()
        {
            return (_LatticeModelResultData as IOptimizationAdaptee<ISwarm>).GetOptimizationObject();
        }

        IParticle IOptimizationAdapter<IParticle>.GetOptimizationObject()
        {
            throw new NotImplementedException();
        }
    }


}
