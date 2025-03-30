using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Domain;
using Newtonsoft.Json;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BookingApi.Create;

public class Function
{
    public async Task<APIGatewayHttpApiV2ProxyResponse> CreateBookingAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        try
        {
            var bookingRequest = JsonConvert.DeserializeObject<BookingDto>(request.Body);

            if (bookingRequest == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Invalid request",
                };
            }

            var guid = Guid.NewGuid();
            bookingRequest.Pk = guid;
            bookingRequest.Sk = guid;
            bookingRequest.Id = guid;

            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            DynamoDBContext dbContext = new DynamoDBContext(client);

            await dbContext.SaveAsync(bookingRequest);
            var message = $"Booking with Id {bookingRequest?.Id} Created";

            LambdaLogger.Log(message);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                Body = message,
                StatusCode = (int)HttpStatusCode.Created
            };
        }
        catch (Exception ex)
        {
            LambdaLogger.Log(ex.Message);
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = "An error occurred while creating the booking"
            };

        }
    }
}
