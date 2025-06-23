using MassTransit;
using MassTransit.Internals;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Sample.Components;

public class CustomerNumberPartitionKeyFilter<T> :
    IFilter<SendContext<T>>,
    IFilter<PublishContext<T>>
    where T : class
{
    private static readonly ReadOnlyProperty<T, string>? _property;

    static CustomerNumberPartitionKeyFilter()
    {
        if (IsCustomerMessage(out PropertyInfo? propertyInfo))
        {
            _property = new ReadOnlyProperty<T, string>(propertyInfo);
        }
    }

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        SetPartitionKey(context);

        return next.Send(context);
    }

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        SetPartitionKey(context);

        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }

    private static void SetPartitionKey(SendContext<T> context)
    {
        if (_property == null)
        {
            return;
        }

        string customerNumber = _property.GetProperty(context.Message);

        if (!string.IsNullOrWhiteSpace(customerNumber))
        {
            context.TrySetPartitionKey(customerNumber);
        }
    }

    private static bool IsCustomerMessage([NotNullWhen(true)] out PropertyInfo? propertyInfo)
    {
        propertyInfo = typeof(T).GetProperty("CustomerNumber");

        return propertyInfo != null;
    }
}