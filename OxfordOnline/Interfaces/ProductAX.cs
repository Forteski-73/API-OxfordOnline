using System;
using System.Collections.Generic;
using System.ServiceModel;
using OxfordOnline.Models;
using Oxfordonline.Integration;

namespace OxfordOnline.Interfaces
{
    public class ProductAX : ProductInterface
    {
        private List<ProdutosAX> produtos = new List<ProdutosAX>();

        public ProductAX()
        {
            Add(new ProdutosAX { Id = "1", NomeDPA = "Nome DPA 1", Decoracao = "Decoracao 1", Marca = "Marca 1" });
            Add(new ProdutosAX { Id = "2", NomeDPA = "Nome DPA 2", Decoracao = "Decoracao 2", Marca = "Marca 1" });
            Add(new ProdutosAX { Id = "3", NomeDPA = "Nome DPA 3", Decoracao = "Decoracao 3", Marca = "Marca 3" });
            Add(new ProdutosAX { Id = "4", NomeDPA = "Nome DPA 4", Decoracao = "Decoracao 4", Marca = "Marca 4" });
            Add(new ProdutosAX { Id = "5", NomeDPA = "Nome DPA 5", Decoracao = "Decoracao 5", Marca = "Marca 5" });
            Add(new ProdutosAX { Id = "6", NomeDPA = "Nome DPA 6", Decoracao = "Decoracao 6", Marca = "Marca 6" });
        }

        public ProdutosAX Add(ProdutosAX item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            produtos.Add(item);
            return item;
        }

        public ProdutosAX Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            // Configuração WCF
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress("http://ax201203:8201/DynamicsAx/Services/WSIntegratorServices");

            var client = new ProductServicesClient(binding, endpoint);
            client.ClientCredentials.Windows.ClientCredential.Domain = "oxford";
            client.ClientCredentials.Windows.ClientCredential.UserName = "svc.aos";
            client.ClientCredentials.Windows.ClientCredential.Password = "svcax2012";

            var AxDocumentContext = new CallContext { Company = "100" };
            var productContract = client.find(AxDocumentContext, id);

            if (productContract == null)
                return null;

            return new ProdutosAX
            {
                Id = productContract.ItemDPA,
                NomeDPA = productContract.NameDPA,
                Marca = productContract.MSBProdBrand,
                Decoracao = productContract.MSBProdDecoration
            };
        }

        public IEnumerable<ProdutosAX> GetAll()
        {
            return produtos;
        }

        public void Remove(string id)
        {
            produtos.RemoveAll(p => p.Id == id);
        }

        public bool Update(ProdutosAX item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            int index = produtos.FindIndex(p => p.Id == item.Id);
            if (index == -1)
                return false;

            produtos[index] = item;
            return true;
        }
    }
}
