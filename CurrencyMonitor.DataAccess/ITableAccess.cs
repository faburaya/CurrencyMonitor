
namespace CurrencyMonitor.DataAccess
{
    public interface ITableAccess<DataType>
    {
        void Insert(DataType obj);

        bool IsEmpty();

        void Commit();
    }
}
