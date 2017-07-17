using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;

namespace StructureMapIoc
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             //Setting in constructor
            var container = new Container(x => x.For<ICreditCard>().Use<MasterCard>().Named("mastercard"));

            //Setting individually
            //container.Configure(x => x.For<ICreditCard>().Use<MasterCard>());
            container.Configure(x => x.For<ICreditCard>().Use<Visa>().Named("visa"));

            //Selecting the Creditcard that I want use
            //var creditCard = container.GetInstance<ICreditCard>("mastercard");
            //Console.WriteLine(creditCard.Charge());

            var shopper = container.GetInstance<Shopper>();
            shopper.Charge();
            */
            /* With the Singleton() the container is the same
             * var container = new Container(x => x.For<ICreditCard>().Singleton().Use<MasterCard>());
             * */

            //With the LifecyclesIs() you can specify how the container is work
            var container = new Container(new MyRegistry());


            var shopper = container.GetInstance<Shopper>();
            shopper.Charge();
            Console.WriteLine(shopper.ChargesForCurrentCard);

            var shopper2 = container.GetInstance<Shopper>();
            shopper2.Charge();
            Console.WriteLine(shopper2.ChargesForCurrentCard); ;

            Console.Read();
        }
        public class MyRegistry : Registry
        {
            public MyRegistry()
            {
                //With the LifecyclesIs() you can specify how the container is work
                For<ICreditCard>().LifecycleIs(null).Use<MasterCard>();
            }
        }

        public class Resolver
        {
            private Dictionary<Type, Type> dependencyMap = new Dictionary<Type, Type>();

            public T Resolve<T>()
            {
                return (T)Resolve(typeof(T));
            }

            public void Register<TFrom, TTo>()
            {
                dependencyMap.Add(typeof(TFrom), typeof(TTo));
            }

            private object Resolve(Type typeToResolve)
            {
                Type resolvedType = null;

                try
                {
                    resolvedType = dependencyMap[typeToResolve];
                }
                catch
                {
                    throw new Exception(string.Format("Could not resolve type {0}", typeToResolve.FullName));
                }

                var firstContructor = resolvedType.GetConstructors().First();
                var contructorParameters = firstContructor.GetParameters();

                if (contructorParameters.Count() == 0)
                    return Activator.CreateInstance(resolvedType);

                IList<object> parameters = new List<object>();
                foreach (var parameterToResolve in contructorParameters)
                {
                    parameters.Add(Resolve(parameterToResolve.ParameterType));
                }
                return firstContructor.Invoke(parameters.ToArray());
            }
        }

        public class Visa : ICreditCard
        {
            public int ChargeCount { get { return 0; } }

            public string Charge()
            {
                return "Charging the Visa!";
            }
        }

        public class MasterCard : ICreditCard
        {
            public int ChargeCount { get; set; }

            public string Charge()
            {
                ChargeCount++;
                return "Swiping the MasterCard";
            }
        }

        public interface ICreditCard
        {
            string Charge();
            int ChargeCount { get; }
        }

        public class Shopper
        {
            private readonly ICreditCard _creditCard;

            public Shopper(ICreditCard creditCard)
            {
                this._creditCard = creditCard;
            }

            public int ChargesForCurrentCard
            {
                get { return this._creditCard.ChargeCount; }
            }

            public void Charge()
            {
                var chargeMessage = _creditCard.Charge();
                Console.WriteLine(chargeMessage);
            }
        }
    }    
}
