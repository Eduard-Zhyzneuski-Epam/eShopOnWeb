using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderReserveService : IOrderReserveService
{
    private ServiceBusClient serviceBusClient;
    private HttpClient httpClient;
    private string orderReserveUrl;

    public OrderReserveService(IHttpClientFactory httpClientFactory, CatalogSettings settings)
    {
        serviceBusClient = new ServiceBusClient(settings.ServiceBusConnectionString);
        httpClient = httpClientFactory.CreateClient("OrderReserver");
        orderReserveUrl = settings.DeliverReserveFunctionUrl;
    }

    public async Task ReserveOrder(Order order)
    {
        var orderJson = JsonConvert.SerializeObject(order);

        var sender = serviceBusClient.CreateSender("orders_reservation");
        await sender.SendMessageAsync(new ServiceBusMessage(orderJson));

        await httpClient.PostAsync(orderReserveUrl, new StringContent(orderJson));
    }
}
