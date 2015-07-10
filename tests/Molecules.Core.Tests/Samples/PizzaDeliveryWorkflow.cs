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
        Busy,
        Failed
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

    public interface IStore
    {
        InStoreStatus DispatchToStore(Store store, Delivery delivery);

        InStoreStatus CheckStatus(string orderId);
    }

    public interface ICustomer
    {
        InStoreStatus Notify(InStoreStatus status);
    }

    public interface IPaymentService
    {
        PaymentStatus TakePayment(Payment payment);

        RefundStatus Refund(Payment payment);
    }

    public class PizzaDeliveryWorkflow
    {
        public ReceiverAtom<Order, Order> Build()
        {
            return (
                from order in Atom.With<Order>()
                from paymentStatus in
                    Atom.Func(c => c.Resolve<IPaymentService>().TakePayment(order.Payment))
                        .Catch().Wait(20).Seconds.Retry(3).Return(PaymentStatus.Failed)
                from status in
                    paymentStatus == PaymentStatus.Success
                        ? Atom.Func(c =>
                            c.Resolve<IStore>().DispatchToStore(order.Store, order.Delivery))
                        : Atom.Return(InStoreStatus.DidNotReceive)
                from polledStatus in
                    status == InStoreStatus.DidNotReceive
                        ? Atom.Func(c => c.Resolve<IPaymentService>().Refund(order.Payment))
                            .Catch().Wait(20).Seconds.Retry(3).Return(status)
                        : Atom.Func(c =>
                            c.Resolve<IStore>()
                                .CheckStatus(order.Id))
                            .Catch()
                            .Wait(30)
                            .Seconds.Retry(3)
                            .While(s => s != InStoreStatus.OnItsWay)
                            .Do(c => c.Resolve<ICustomer>().Notify(c.Input))
                select order)
                .Receiver()
                .Listen<Order>();
        }
    }
}