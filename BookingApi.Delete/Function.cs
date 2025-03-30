using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Domain;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BookingApi.Delete;

public class Function
{
    public async Task<APIGatewayHttpApiV2ProxyResponse> DeleteBookingAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
       try
        {
            if (request == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.BadRequest,
                    Body = "Invalid request"
                };
            }

            string idFromPath = request.PathParameters["id"];

            if (string.IsNullOrEmpty(idFromPath))
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = "Invalid booking id"
                };
            }

            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            DynamoDBContext dbContext = new DynamoDBContext(client);

            Guid id = new Guid(idFromPath);
            var existingBooking = await dbContext.LoadAsync<BookingDto>(id, id);

            if (existingBooking is null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = "No booking to delete",
                    StatusCode = (int) HttpStatusCode.BadRequest
                };
            };

            await dbContext.DeleteAsync<BookingDto>(existingBooking);

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = $"Booking with Id {existingBooking.Id} Deleted"
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"Error: {ex.Message}");
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = (int) HttpStatusCode.InternalServerError,
                Body = "Internal Server Error"
            };
        }
    }


}
