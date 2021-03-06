using System;
using System.Collections.Generic;
using Bangazon.Models;
using Microsoft.Data.Sqlite;

namespace Bangazon.Managers
{
    /*  OrderManager completes all methods against the order table
        Authored by Jason Smith */
    public class OrderManager
    {
        private List<Order> _orders;
        private DatabaseInterface _db;

        /*  Instantiate OrderManager passing a refernce to the db interface
            Authored by Jason Smith */
        public OrderManager(DatabaseInterface db)
        {
            _db = db;
            _orders = new List<Order>();
        }

        /*  Checks if customer has an open order, if not add an order to the database and return the id, which is the PK where it was placed, 
            either way the product will be add to the product order table for that order
            Authored by Jason Smith */
        public int CreateOrder(int prodId, int custId)
        {
            int index = 0;
            _db.Query($"SELECT orderId FROM [order] WHERE customerId = {custId} AND paymentTypeId IS NULL", (SqliteDataReader reader) => {
                    while(reader.Read()) {
                        index = reader.GetInt32(0);
                    }
                }
            );
            if(index == 0) {
                index = _db.Insert( $"INSERT INTO [order] (orderId, customerId) VALUES (null, {custId})");
            }

            _db.Insert( $"INSERT INTO prodOrder VALUES (null, {index}, {prodId})");
            _orders.Add(
                new Order()
                {
                    id = index,
                    customerId = custId,
                    paymentTypeId = null,
                    dateCreated = DateTime.Now
                }
            );
            return index;
        }

        /*  Return a list of all orders 
            Authored by Jason Smith */
        public List<Order> GetOrders()
        {
            _db.Query($"SELECT orderId, dateCreated, customerId, paymentTypeId FROM [order]", 
                (SqliteDataReader reader) => {
                    _orders.Clear();
                    while (reader.Read())
                    {
                        _orders.Add(
                            new Order(){
                                id = reader.GetInt32(0),
                                dateCreated = reader.GetDateTime(1),
                                customerId = reader.GetInt32(2),
                                paymentTypeId = reader[3] as int? ?? null
                            }
                        );
                    }
                }
            );
            return _orders;            
        }

        /*  Add a payment to the null field "payment" in an order, return true once complete
            Authored by Jason Smith */
         public bool AddPayment(int payId, int orderId)
        {
            _db.Update($"UPDATE [order] SET paymentTypeId = {payId} WHERE [order].orderId = {orderId}");
            return true;
        }

        /*  Add a product by productId to an order by orderId return the index of the item added to the productorder join table
            Authored by Jason Smith */
        public int AddProductToOrder(int prodId, int orderId)
        {
            int index =  _db.Insert( $"INSERT INTO prodOrder VALUES (null, {orderId}, {prodId})");
            return index;
        }

        /*  Return a list that contains orderId, product name, quantity, product price for all items created by the given customer
            Authored by Jason Smith */
        public List<(int, int, string, double)> RevenueReport(int custId)
        {
             List<(int orderId, int quantity, string prodTitle, double price)> _report = new List<(int, int, string, double)>();
            _db.Query($"SELECT o.orderId, Count(p.productId), p.title, p.price FROM [order] o LEFT JOIN prodOrder po ON po.orderId = o.orderId LEFT JOIN product p ON p.productId = po.productId WHERE p.customerId = {custId} GROUP BY po.productId",
                (SqliteDataReader reader) => {
                    while(reader.Read())
                    {
                        _report.Add((reader.GetInt32(0), reader.GetInt32(1), reader[2].ToString(),reader.GetDouble(3)));
                    }
                }
            );
            return _report;
        }

        /*  Return a total for all products in a customers order
            Authored by Jason Smith */
        public Order OrderTotal(int custId)
        {
            
            Order custOrder = new Order();
            custOrder.id = 0;
            _db.Query($"SELECT  p.Price, o.orderId FROM [order] o LEFT JOIN prodOrder po ON po.orderId = o.orderId LEFT JOIN product p ON p.productId = po.productId WHERE o.customerId = {custId} AND o.paymentTypeId IS NULL",
                (SqliteDataReader reader) => {
                    while(reader.Read())
                    {
                        custOrder.total += reader.GetDouble(0);
                        custOrder.id = reader.GetInt32(1);

                    }
                }
            );
            return custOrder;
        }
    }
}
