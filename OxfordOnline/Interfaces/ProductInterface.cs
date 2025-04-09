using OxfordOnline.Models;

namespace OxfordOnline.Interfaces
{
    interface ProductInterface
    {
        IEnumerable<Produtos> GetAll();
        Produtos Get(string id);
        Produtos Add(Produtos item);
        void Remove(string id);
        bool Update(Produtos item);
    }
}