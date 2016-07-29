using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachine
{
    public class ProductState {
        public Product Product { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }
    }

    public class VendingMachine
    {
        public int[] AllowedDenominations = { 1, 5, 10, 20, 50, 100, 200 };

        /// <summary>
        /// Maps an int (the code of the product) to its ProductState
        // (AvailableUnits and Price besides the Product itself)
        /// </summary>
        private Dictionary<int, ProductState> ProductsToSell;

        /// <summary>
        /// The key is Denomination in cents (pences), the value is how many coins are available
        /// </summary>
        private Dictionary<int, CoinChange> AvailableChange;

        private Dictionary<int, CoinChange> InsertedCoins;

        public VendingMachine()
        {
            this.ProductsToSell = new Dictionary<int, ProductState>();
            this.AvailableChange = new Dictionary<int, CoinChange>();
            this.InsertedCoins = new Dictionary<int, CoinChange>();
        }

        public Dictionary<int, ProductState> GetAvailableItems()
        {
            return ProductsToSell;
        }

        public Dictionary<int, CoinChange> GetAvailableChange()
        {
            return AvailableChange;
        }

        #region Vending Machine functions
        private void OutputMessage(string message)
        {
            Console.WriteLine(message);
        }

        private Product DispenseProduct(int productCode)
        {
            // TODO: We should implement here the
            // logic controlling how the hardware product dispenser works
            var product = ProductsToSell[productCode];
            product.Count -= 1;

            return product.Product;
        }

        public List<CoinChange> ReturnChange(List<CoinChange> toReturn)
        {
            // TODO: We should implement here the
            // logic controlling how the hardware change dispenser works
            foreach (var change in toReturn)
            {
                AvailableChange[change.Denomination].Count -= change.Count;
            }

            return toReturn;
        }
        #endregion

        public void ReloadProducts(IDictionary<int, ProductState> productsToReload, ICollection<CoinChange> coinsToReload)
        {

        }

        public void InsertCoin(int coin)
        {
            if (!AllowedDenominations.Contains(coin))
            {
                OutputMessage("UNRECOGNIZED COIN. PLEASE INSERT A VALID COIN.");
            }
            if (InsertedCoins.Keys.Contains(coin))
            {
                InsertedCoins[coin].Count += 1;
            }
            else
            {
                InsertedCoins[coin] = new CoinChange()
                {
                    Denomination = coin,
                    Count = 1
                };
            }

            OutputMessage("INSERTED AMOUNT: " + GetInsertedAmount() + "p");
        }

        private int GetInsertedAmount()
        {
            var total = 0;
            foreach (int denomination in InsertedCoins.Keys)
            {
                var currentCoin = InsertedCoins[denomination];
                total += currentCoin.Denomination * currentCoin.Count;
            }
            return total;
        }

        public Product ProcessChoice(int productCode)
        {
            if (!ProductsToSell.Keys.Contains(productCode) || ProductsToSell[productCode].Count == 0)
            {
                OutputMessage("INVALID CODE");
                return null;
            }

            var product = ProductsToSell[productCode];
            var insertedCoins = GetInsertedAmount();
            if (product.Price > insertedCoins)
            {
                OutputMessage("PLEASE INSERT MORE COINS");
                return null;
            }

            var changeToReturn = GetChange(insertedCoins, product);

            return DispenseProduct(productCode);
        }

        private List<CoinChange> GetChange(int payment, ProductState acquiredProduct)
        {
            var totalChange = payment - acquiredProduct.Price;
            return CalculateChange(
                totalChange,
                new List<CoinChange>(),
                AvailableChange.Keys.OrderByDescending(k => k).ToList());
        }

        private List<CoinChange> CalculateChange(
            int change, List<CoinChange> actualChange, List<int> availableDenominations)
        {

            // we emptied the available denominations
            if (availableDenominations.Count == 0) {
                return actualChange;
            }

            // get the biggest denomination available for change (available denominations in descending order)
            var currentDenomination = availableDenominations.First();
            // if the change we want is smaller than this denomination,
            // we can't use it for change
            if (change < currentDenomination)
            {
                return actualChange;
            }

            // how many coins you can sum before reaching the change
            var numberOfCoinsToUse = change / currentDenomination;

            if (numberOfCoinsToUse > AvailableChange[currentDenomination].Count)
            {
                // we can only use the available change on the machine
                numberOfCoinsToUse = AvailableChange[currentDenomination].Count;
            }

            actualChange.Add(new CoinChange() {
                Denomination = currentDenomination,
                Count = numberOfCoinsToUse
            });

            // we now just have to calculate the remaining change
            change -= numberOfCoinsToUse * currentDenomination;

            // removes the used denomination
            availableDenominations.RemoveAt(0);

            if (change > 0)
            {
                // we add to the actual partial change we calculated, the remaining calculated change
                actualChange.AddRange(CalculateChange(change, actualChange, availableDenominations));
            }

            return actualChange;
        }
    }
}
