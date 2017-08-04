using System;
using Xunit;
using Bangazon.Models;
using Bangazon.Managers;

namespace Bangazon.Tests
{
    public class CustomerManagerShould
    {
        private readonly CustomerManager _register;

        public CustomerManagerShould()
        {
            _register = new CustomerManager();
        }


        [Theory]
        [InlineData("Sarah Jones", "787878", "Nash", "TN", "37128", "615-676-6767")]
  
        public void AddNewCustomer(string name, string streetAddress, string city, string state, string zip, string phone)
        {
            var result = _register.AddCustomer(name, streetAddress, city, state, zip, phone);
            Assert.True(result);
        }

        [Fact]
        public void ListCustomers()
        {

        }
    }
}
