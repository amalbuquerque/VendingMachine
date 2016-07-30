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
        private string OutputMessage(string message)
        {
            Console.WriteLine(message);
            return message;
        }

        private Product DispenseProduct(int productCode)
        {
            // TODO: We should implement here the
            // logic controlling how the hardware product dispenser works
            var product = ProductsToSell[productCode];
            product.Count -= 1;

            return product.Product;
        }

        //public List<CoinChange> ReturnChange(List<CoinChange> toReturn)
        //{
        //    foreach (var change in toReturn)
        //    {
        //        AvailableChange[change.Denomination].Count -= change.Count;
        //    }

        //    return toReturn;
        //}

        public List<CoinChange> ReturnInsertedCoins()
        {
            // TODO: We should implement here the
            // logic controlling how the hardware coins dispenser works
            var toReturn = InsertedCoins.Values.ToList();
            InsertedCoins.Clear();
            return toReturn;
        }
        #endregion

        /// <summary>
        /// It receives a map associating a product code to a productState (Product, Count, Price).
        /// If there is already a product with a specific code 
        /// and the product reloaded is the same, it increments its counter.
        /// If the existent and the reloaded products aren't the same, it doesn't allow it.
        /// </summary>
        /// <param name="productsToReload"></param>
        public void ReloadProducts(IDictionary<int, ProductState> productsToReload)
        {
            foreach (var productCode in productsToReload.Keys)
            {
                if (ProductsToSell.ContainsKey(productCode))
                {
                    var existentProduct = ProductsToSell[productCode];
                    var productToReload = productsToReload[productCode];

                    // if there are already products in the dispenser and the products aren't the same
                    if (existentProduct.Count > 0 &&
                        !string.Equals(existentProduct.Product.Description, productToReload.Product.Description))
                    {
                        throw new ArgumentException("You can't sell: " + productToReload.Product.Description
                            + " with code: " + productCode + " because there is already " + existentProduct.Count
                            + " units of: " + existentProduct.Product.Description + " in that dispenser.");
                    }
                    // the price may have changed with the reload
                    existentProduct.Price = productToReload.Price;
                    // reload the new items
                    existentProduct.Count += productToReload.Count;
                }
                else
                {
                    ProductsToSell[productCode] = productsToReload[productCode];
                }
            }
        }

        public void ReloadChange(ICollection<CoinChange> coinsToReload)
        {
            foreach (var change in coinsToReload)
            {
                var denomination = change.Denomination;
                if (AvailableChange.ContainsKey(denomination)) {
                    AvailableChange[denomination].Count += change.Count;
                }
                AvailableChange[denomination] = change;
            }
        }

        public string InsertCoin(int coin)
        {
            if (!AllowedDenominations.Contains(coin))
            {
                return OutputMessage("UNRECOGNIZED COIN. PLEASE INSERT A VALID COIN.");
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

            return OutputMessage("INSERTED AMOUNT: " + GetInsertedAmount() + "p");
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

            // we have enough coins to dispense the product
            // we empty the inserted coins into the available coins (we accept the inserted coins)
            TurnInsertedCoinsIntoAvailableChange();

            // we have to return this change, so the coins are put
            // in the InsertedCoins place, where the user can retrieve them or use
            // them for other product
            var changeToReturn = GetChange(insertedCoins, product);
            InsertedCoins = changeToReturn.ToDictionary(c => c.Denomination, c => c);

            return DispenseProduct(productCode);
        }

        private void TurnInsertedCoinsIntoAvailableChange()
        {
            foreach (var denomination in InsertedCoins.Keys.ToArray())
            {
                if (!AvailableChange.ContainsKey(denomination))
                {
                    AvailableChange[denomination] = InsertedCoins[denomination];
                }
                else
                {
                    AvailableChange[denomination].Count += InsertedCoins[denomination].Count;
                }
                InsertedCoins.Remove(denomination);
            }
        }

        private List<CoinChange> GetChange(int payment, ProductState acquiredProduct)
        {
            var totalChange = payment - acquiredProduct.Price;
            var changeToReturn = CalculateChange(
                totalChange,
                new List<CoinChange>(),
                AvailableChange.Keys.OrderByDescending(k => k).ToList());

            // we have to take this change from the available coins
            foreach (var coin in changeToReturn)
            {
                AvailableChange[coin.Denomination].Count -= coin.Count;
            }
            return changeToReturn;
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
