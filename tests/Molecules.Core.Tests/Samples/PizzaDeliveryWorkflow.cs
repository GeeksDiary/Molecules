using System;
using System.Collections.Generic;

namespace Molecules.Core.Tests.Samples
{
    public class Item
    {
    }

    public class Delivery
    {
    }

    public class Payment
    {
    }

    public class Store
    {
    }

    public enum PaymentStatus
    {
        Success,
        InsufficientFunds,
        Busy
    }

    public enum InStoreStatus
    {
        Accepted,
        Making,
        InOven,
        OnItsWay,
        Delivered,
        DidNotReceive
    }

    public enum RefundStatus
    {
        Success,
        Failed,
        Busy
    }

    public class Order
    {
        public string Id { get; set; }

        public IEnumerable<Item> Items { get; set; }

        public Delivery Delivery { get; set; }

        public Payment Payment { get; set; }
        public Store Store { get; set; }
    }

    public static class Services
    {
        public static PaymentStatus TakePayment(Payment payment)
        {
            throw new NotImplementedException();
        }

        public static InStoreStatus DispatchToStore(Store store, Delivery delivery)
        {
            throw new NotImplementedException();
        }

        public static RefundStatus Refund(Payment payment)
        {
            throw new NotImplementedException();
        }

        public static InStoreStatus CheckStatus(string orderId)
        {
            throw new NotImplementedException();
        }

        public static InStoreStatus Notify(InStoreStatus status)
        {
            throw new NotImplementedException();
        }
    }

    public class PizzaDeliveryWorkflow
    {
        public Atom<Order> Build()
        {
            return (
                from order in Atom.With<Order>()
                from paymentStatus in
                    Atom.Of(() => Services.TakePayment(order.Payment))
                        .Retry(3)
                        .After(TimeSpan.FromMinutes(1))
                from status in
                    paymentStatus == PaymentStatus.Success
                        ? Atom.Of(() => Services.DispatchToStore(order.Store, order.Delivery))
                        : Atom.Of(() => InStoreStatus.DidNotReceive)
                from polledStatus in
                    status == InStoreStatus.DidNotReceive
                        ? Atom.Of(() => Services.Refund(order.Payment))
                            .Retry(3)
                            .Return(status)
                        : Atom.Of(() => Services.CheckStatus(order.Id))
                            .While(s => s == InStoreStatus.OnItsWay)
                            .Do(s => Services.Notify(s))
                select order)
                .AsReceivable()
                .Of<Order>();
        }
    }
}