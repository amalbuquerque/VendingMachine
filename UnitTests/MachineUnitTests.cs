using System;
using NUnit.Framework;
using VM = VendingMachine;
using System.Collections.Generic;

namespace UnitTests
{
    [TestFixture]
    public class MachineUnitTests
    {

        private VM.VendingMachine Machine;

        [SetUp]
        public void SetUp()
        {
            Machine = new VM.VendingMachine();
        }

        [Test]
        public void TestReloadProductsInitiallyEmpty()
        {
            var productsToReload = new Dictionary<int, VM.ProductState>() {
                { 1 , new VM.ProductState() {
                    Product = new VM.Product() { Description = "Water" },
                    Count = 5,
                    Price = 100
                    }
                },
                { 2 , new VM.ProductState() {
                    Product = new VM.Product() { Description = "Snack" },
                    Count = 15,
                    Price = 150
                    }
                }
           };

            var changeToReload = new List<VM.CoinChange>() {
                new VM.CoinChange() {
                    Denomination = 5,
                    Count = 50
                },
                new VM.CoinChange() {
                    Denomination = 50,
                    Count = 50
                }
           };

            Machine.ReloadProducts(productsToReload);
            Machine.ReloadChange(changeToReload);

            Assert.IsNotNull(Machine.GetAvailableItems());
            Assert.IsNotNull(Machine.GetAvailableChange());
            var products = Machine.GetAvailableItems();
            var change = Machine.GetAvailableChange();
            Assert.AreEqual(products.Count, 2);
            Assert.AreEqual(products[2].Product.Description, "Snack");
            Assert.AreEqual(products[1].Price, 100);
            Assert.AreEqual(products[1].Count, 5);
            Assert.AreEqual(products[2].Price, 150);
            Assert.AreEqual(products[2].Count, 15);
        }

        [Test]
        public void TestReloadProductsInitiallyWithProducts()
        {
            // Reloads 33 water, 10 snack and 44 colas
            TestReloadProductsInitiallyEmpty();
            var newProducts = new Dictionary<int, VM.ProductState>() {
                { 1 , new VM.ProductState() {
                    Product = new VM.Product() { Description = "Water" },
                    Count = 33,
                    Price = 88
                    }
                },
                { 2 , new VM.ProductState() {
                    Product = new VM.Product() { Description = "Snack" },
                    Count = 10,
                    Price = 150
                    }
                },
                { 5 , new VM.ProductState() {
                    Product = new VM.Product() { Description = "Cola" },
                    Count = 44,
                    Price = 120
                    }
                }
            };
            Machine.ReloadProducts(newProducts);
            Assert.IsNotNull(Machine.GetAvailableItems());
            Assert.IsNotNull(Machine.GetAvailableChange());
            var products = Machine.GetAvailableItems();
            var change = Machine.GetAvailableChange();

            Assert.AreEqual(products.Count, 3);
            Assert.AreEqual(products[2].Product.Description, "Snack");
            Assert.AreEqual(products[2].Price, 150);
            Assert.AreEqual(products[2].Count, 25);

            Assert.AreEqual(products[1].Product.Description, "Water");
            Assert.AreEqual(products[1].Price, 88);
            Assert.AreEqual(products[1].Count, 38);

            Assert.AreEqual(products[5].Product.Description, "Cola");
            Assert.AreEqual(products[5].Price, 120);
            Assert.AreEqual(products[5].Count, 44);

            Assert.Catch<KeyNotFoundException>(() => { var unknown = Machine.GetAvailableItems()[4]; });
        }

        [Test]
        public void TestReloadProductsInitiallyWithDifferentProducts()
        {
            // Reloads 33 water, 10 snack and 44 colas
            TestReloadProductsInitiallyEmpty();
            var newProducts = new Dictionary<int, VM.ProductState>() {
                { 1 , new VM.ProductState() {
                    Product = new VM.Product() { Description = "Gummy bears" },
                    Count = 33,
                    Price = 88
                    }
                },
                { 2 , new VM.ProductState() {
                    Product = new VM.Product() { Description = "Chocolate cookies" },
                    Count = 10,
                    Price = 222
                    }
                }
            };
            // no reload will be made
            Assert.Catch<ArgumentException>(() => Machine.ReloadProducts(newProducts));

            newProducts[1].Product.Description = "Water";
            // it reloads only the water
            Assert.Catch<ArgumentException>(() => Machine.ReloadProducts(newProducts));
            Assert.AreEqual(Machine.GetAvailableItems()[1].Product.Description, "Water");
            Assert.AreEqual(Machine.GetAvailableItems()[1].Count, 38);
            Assert.AreEqual(Machine.GetAvailableItems()[1].Price, 88);

            Assert.AreEqual(Machine.GetAvailableItems()[2].Product.Description, "Snack");
            Assert.AreEqual(Machine.GetAvailableItems()[2].Count, 15);
            Assert.AreEqual(Machine.GetAvailableItems()[2].Price, 150);

            newProducts[2].Product.Description = "Snack";
            // it reloads the water again and the snack
            Assert.DoesNotThrow(() => Machine.ReloadProducts(newProducts));
            Assert.AreEqual(Machine.GetAvailableItems()[1].Product.Description, "Water");
            Assert.AreEqual(Machine.GetAvailableItems()[1].Count, 71);
            Assert.AreEqual(Machine.GetAvailableItems()[1].Price, 88);

            Assert.AreEqual(Machine.GetAvailableItems()[2].Product.Description, "Snack");
            Assert.AreEqual(Machine.GetAvailableItems()[2].Count, 25);
            Assert.AreEqual(Machine.GetAvailableItems()[2].Price, 222);
        }

        private int TotalAmountCoins(ICollection<VM.CoinChange> change)
        {
            var total = 0;
            foreach (var coin in change)
            {
                total += coin.Denomination * coin.Count;
            }
            return total;
        }

        [Test]
        public void TestInsertAndRetrieveCoins()
        {
            // Starts with 5 water (100p), 15 snack (150p)
            // And 50 coins of 5p; And 50 coins of 50p
            TestReloadProductsInitiallyEmpty();

            // inserts a pound
            Machine.InsertCoin(100);

            var returnedCoins = Machine.ReturnInsertedCoins();

            Assert.AreEqual(100, TotalAmountCoins(returnedCoins));
        }

        [Test]
        public void TestProcessChoiceEnoughMoney()
        {
            // Starts with 5 water (100p), 15 snack (150p)
            // And 50 coins of 5p; And 50 coins of 50p
            TestReloadProductsInitiallyEmpty();

            var initialWaterStock = Machine.GetAvailableItems()[1].Count;

            // inserts a pound
            Machine.InsertCoin(100);

            // we choose a water
            var product = Machine.ProcessChoice(1);

            var returnedCoins = Machine.ReturnInsertedCoins();

            Assert.AreEqual(0, TotalAmountCoins(returnedCoins));
            Assert.IsNotNull(product);
            Assert.AreEqual(product.Description, "Water");
            Assert.AreEqual(Machine.GetAvailableItems()[1].Count, initialWaterStock - 1);
            Assert.AreEqual(Machine.GetAvailableChange()[50].Count, 50);
            Assert.AreEqual(Machine.GetAvailableChange()[5].Count, 50);
            Assert.AreEqual(Machine.GetAvailableChange()[100].Count, 1);
        }

        [Test]
        public void TestProcessChoiceEnoughMoneyWithChange()
        {
            // Starts with 5 water (100p), 15 snack (150p)
            // And 50 coins of 5p; And 50 coins of 50p
            TestReloadProductsInitiallyEmpty();

            var initialWaterStock = Machine.GetAvailableItems()[1].Count;

            // inserts 3 pounds
            Machine.InsertCoin(50);
            Machine.InsertCoin(50);
            Machine.InsertCoin(200);

            // we choose a water
            var product = Machine.ProcessChoice(1);

            var returnedCoins = Machine.ReturnInsertedCoins();

            Assert.AreEqual(200, TotalAmountCoins(returnedCoins));
            Assert.IsNotNull(product);
            Assert.AreEqual(product.Description, "Water");
            Assert.AreEqual(Machine.GetAvailableItems()[1].Count, initialWaterStock - 1);
            Assert.AreEqual(Machine.GetAvailableChange()[50].Count, 52);
            Assert.AreEqual(Machine.GetAvailableChange()[5].Count, 50);
            Assert.AreEqual(Machine.GetAvailableChange()[200].Count, 0);
        }

        [Test]
        public void TestProcessChoiceEnoughMoneyWithChange2()
        {
            // Water is now 88p
            TestReloadProductsInitiallyWithProducts();

            var changeToReload = new List<VM.CoinChange>() {
                new VM.CoinChange() {
                    Denomination = 5,
                    Count = 50
                },
                new VM.CoinChange() {
                    Denomination = 50,
                    Count = 50
                }
            };
            Machine.ReloadChange(changeToReload);

            var initialWaterStock = Machine.GetAvailableItems()[1].Count;

            // inserts 3 pounds
            Machine.InsertCoin(50);
            Machine.InsertCoin(50);
            Machine.InsertCoin(200);

            // we choose a water
            var product = Machine.ProcessChoice(1);

            // it will only be able to return 210p,
            // instead of the correct 212p
            var returnedCoins = Machine.ReturnInsertedCoins();

            Assert.AreEqual(210, TotalAmountCoins(returnedCoins));
            Assert.IsNotNull(product);
            Assert.AreEqual(product.Description, "Water");
            Assert.AreEqual(Machine.GetAvailableItems()[1].Count, initialWaterStock - 1);
            Assert.AreEqual(Machine.GetAvailableChange()[50].Count, 52);
            Assert.AreEqual(Machine.GetAvailableChange()[5].Count, 48);
            Assert.AreEqual(Machine.GetAvailableChange()[200].Count, 0);
        }

        [Test]
        public void TestProcessChoiceInsufficientFunds()
        {
            // Starts with 5 water (100p), 15 snack (150p)
            // And 50 coins of 5p; And 50 coins of 50p
            TestReloadProductsInitiallyEmpty();

            var initialWaterStock = Machine.GetAvailableItems()[1].Count;

            // inserts 80p
            Machine.InsertCoin(50);
            Machine.InsertCoin(20);
            Machine.InsertCoin(10);

            // we choose a water
            var product = Machine.ProcessChoice(1);

            var returnedCoins = Machine.ReturnInsertedCoins();

            Assert.AreEqual(80, TotalAmountCoins(returnedCoins));
            Assert.IsNull(product);
            Assert.AreEqual(Machine.GetAvailableItems()[1].Count, initialWaterStock);
            Assert.AreEqual(Machine.GetAvailableChange()[50].Count, 50);
            Assert.AreEqual(Machine.GetAvailableChange()[5].Count, 50);
        }
    }
}
