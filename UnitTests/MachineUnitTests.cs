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

        [PreTest]
        public void SetUp()
        {
            Machine = new VM.VendingMachine();
        }

        [Test]
        public void TestSetReloadProductsInitiallyEmpty()
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
                },
            };

            Machine.ReloadProducts(productsToReload, changeToReload);

            Assert.IsNotNull(Machine.GetAvailableItems());
            Assert.IsNotNull(Machine.GetAvailableChange());
            var products = Machine.GetAvailableItems();
            var change = Machine.GetAvailableChange();
            Assert.AreEqual(products.Count, 2);
            Assert.AreEqual(products[2].Product.Description, "Snack");
            Assert.AreEqual(products[1].Price, 50);
        }

        [Test]
        public void TestSetReloadProductsInitiallyWithProducts()
        {
        }

        [Test]
        public void TestProcessChoiceEnoughMoney()
        {
        }
        
        [Test]
        public void TestProcessChoiceInsufficientFunds()
        {
        }
    }
}
