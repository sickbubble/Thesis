
namespace Adapters
{
    public interface IAdapter
    {
        IAdaptee Adaptee { get; }
    }

    public interface IOptimizationAdapter<T> : IAdapter 
    {
        T GetOptimizationObject(); // convert to optimization method interface
    }

 
    // Adaptee interface
    public interface IAdaptee
    {
    }

    public interface IOptimizationAdaptee<T> : IAdaptee
    {
        T GetOptimizationObject();

    }



    


}