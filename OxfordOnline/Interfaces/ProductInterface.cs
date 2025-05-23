using OxfordOnline.Models;

namespace OxfordOnline.Interfaces
{
    public interface ProductInterface
    {
        IEnumerable<ProdutosAX> GetAll();
        ProdutosAX Get(string id);
        ProdutosAX Add(ProdutosAX item);
        void Remove(string id);
        bool Update(ProdutosAX item);
    }
}