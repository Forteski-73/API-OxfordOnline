using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OxfordOnline.Interfaces;
using OxfordOnline.Models;
using Oxfordonline.Integration;
using System.ServiceModel;

namespace OxfordOnline.Interfaces
{
    public class Product : ProductInterface
    {
        private List<Produtos> produtos = new List<Produtos>();


        public Product()
        {
            Add(new Produtos { NomeDPA = "Nome DPA 1", Decoracao = "Decoracao 1", Marca = "Marca 1" });
            Add(new Produtos { NomeDPA = "Nome DPA 2", Decoracao = "Decoracao 2", Marca = "Marca 1" });
            Add(new Produtos { NomeDPA = "Nome DPA 3", Decoracao = "Decoracao 3", Marca = "Marca 3" });
            Add(new Produtos { NomeDPA = "Nome DPA 4", Decoracao = "Decoracao 4", Marca = "Marca 4" });
            Add(new Produtos { NomeDPA = "Nome DPA 5", Decoracao = "Decoracao 5", Marca = "Marca 5" });
            Add(new Produtos { NomeDPA = "Nome DPA 6", Decoracao = "Decoracao 6", Marca = "Marca 6" });
        }

        public Produtos Add(Produtos item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            produtos.Add(item);
            return item;
        }
        public Produtos Get(string id)
        {
            string product = id;

            // Cria manualmente a configuração do WCF
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress("http://ax201203:8201/DynamicsAx/Services/WSIntegratorServices"); //8101

            // Criar a instância do cliente WCF
            ProductServicesClient client = new ProductServicesClient(binding, endpoint);

            // Configurar credenciais do Windows
            client.ClientCredentials.Windows.ClientCredential.Domain = "oxford";
            client.ClientCredentials.Windows.ClientCredential.UserName = "svc.aos";
            client.ClientCredentials.Windows.ClientCredential.Password = "svcax2012";

            // Criar contexto e chamada do serviço
            CallContext AxDocumentContext = new CallContext { Company = "100" };
            ProdutctContract productContract = client.find(AxDocumentContext, product);

            Produtos productModel = new Produtos();

            if (productContract != null)
            {
                productModel.Id = productContract.ItemDPA;
                productModel.NomeDPA = productContract.NameDPA;
                productModel.Marca = productContract.MSBProdBrand;
                productModel.Decoracao = productContract.MSBProdDecoration;
            }

            return productModel;
        }


        public IEnumerable<Produtos> GetAll()
        {
            return produtos;
        }

        public void Remove(string id)
        {
            produtos.RemoveAll(p => p.Id == id);
        }

        public bool Update(Produtos item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            int index = produtos.FindIndex(p => p.Id == item.Id);

            if (index == -1)
            {
                return false;
            }
            produtos.RemoveAt(index);
            produtos.Add(item);
            return true;
        }
    }
}