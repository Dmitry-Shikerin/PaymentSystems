using System;
using System.Text;
using System.Security.Cryptography;

namespace Платежные_системы
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Выведите платёжные ссылки для трёх разных систем платежа: 
            //pay.system1.ru/order?amount=12000RUB&hash={MD5 хеш ID заказа}
            //order.system2.ru/pay?hash={MD5 хеш ID заказа + сумма заказа}
            //system3.com/pay?amount=12000&curency=RUB&hash={SHA-1 хеш сумма заказа + ID заказа + секретный ключ от системы}

            MD5Hash mD5Hash = new MD5Hash();
            SHA1Hash sHA1Hash = new SHA1Hash();

            Order order = new Order(1, 12000);

            IPaymentSystem[] paymentSystems = new IPaymentSystem[]
            {
                new PaymentSystem1(mD5Hash),
                new PaymentSystem2(mD5Hash),
                new PaymentSystem3(sHA1Hash, "5"),
            };

            foreach (IPaymentSystem paymentSystem in paymentSystems)
            {
                Console.WriteLine(paymentSystem.GetPayingLink(order));
            }
        }
    }

    public class Order
    {
        public Order(int id, int amount)
        {
            Id = id;
            Amount = amount;
        }

        public int Id { get; }
        public int Amount { get; }
    }

    public interface IPaymentSystem
    {
        string GetPayingLink(Order order);
    }

    class PaymentSystem1 : IPaymentSystem
    {
        private readonly IHashService _hashService;

        public PaymentSystem1(IHashService hashService)
        {
            _hashService = hashService;
        }

        public string GetPayingLink(Order order)
        {
            string orderId = _hashService.GetHash(order.Id.ToString());

            return $"pay.system1.ru/order?amount={order.Amount}RUB&hash={orderId}";
        }
    }

    class PaymentSystem2 : IPaymentSystem
    {
        private readonly IHashService _hashService;

        public PaymentSystem2(IHashService hashService)
        {
            _hashService = hashService;
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashService.GetHash($"{order.Id}{order.Amount}");

            return $"order.system2.ru/pay?hash={hash}";
        }
    }

    class PaymentSystem3 : IPaymentSystem
    {
        private readonly IHashService _hashService;
        private readonly string _secretKey;

        public PaymentSystem3(IHashService hashService, string secretKey)
        {
            _hashService = hashService;
            _secretKey = secretKey;
        }

        public string GetPayingLink(Order order)
        {
            string hash = _hashService.GetHash($"{order.Id}{ order.Amount}{_secretKey}");

            return $"system3.com/pay?amount=12000&curency=RUB&hash={hash}";
        }
    }

    interface IHashService
    {
        string GetHash(string input);
    }

    class MD5Hash : IHashService
    {
        public string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }
    }

    class SHA1Hash : IHashService
    {
        public string GetHash(string input)
        {
            SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));

            return Convert.ToBase64String(hash);
        }
    }
}
