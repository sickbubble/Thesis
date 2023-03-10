
namespace Adapters
{
    public interface IAdapter
    {
        IAdaptee Adaptee { get; }
    }

    public interface IOptimizationAdapter<T> : IAdapter 
    {
        T GetOptimizationOnject(); // convert to optimization method interface
    }

 
    // Adaptee interface
    public interface IAdaptee
    {
    }

    public interface IOptimizationAdaptee<T> : IAdaptee
    {
        T GetOptimizationObject();
    }

    public interface IOptimizationObject 
    {

    }


    // Adapter class
    public class ShellModelOptimizationAdapter : IOptimizationAdapter<IOptimizationObject>
    {

        public ShellModelOptimizationAdapter(IOptimizationAdaptee<IOptimizationObject> adaptee)
        {
            Adaptee = adaptee;
        }


        private IAdaptee _ShellModelResultData;

        public IAdaptee Adaptee { get => _ShellModelResultData; private set => _ShellModelResultData = value; }


        IOptimizationObject IOptimizationAdapter<IOptimizationObject>.GetOptimizationOnject()
        {
            return (_ShellModelResultData as IOptimizationAdaptee<IOptimizationObject>).GetOptimizationObject();
        }
    }



}