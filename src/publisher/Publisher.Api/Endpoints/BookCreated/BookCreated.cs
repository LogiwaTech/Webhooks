using Ardalis.ApiEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Publisher.Contract.Contracts.Envelope;
using Publisher.Contract.Requests.BookCreated;
using System.Net;

namespace Publisher.Api.Endpoints.BookCreated
{
    [Route("/api/book-created")]
    public class BookCreated : EndpointBaseAsync.WithRequest<BookCreatedRequest>.WithActionResult
    {
        private IMediator mediator;
        public BookCreated(IMediator _mediator) => mediator = _mediator;

        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(typeof(Envelope<bool>), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(Envelope<bool>), StatusCodes.Status400BadRequest)]
        public override async Task<ActionResult> HandleAsync([FromBody] BookCreatedRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            var command = Service.Commands.BookCreated.BookCreatedCommand.FromRequest(request);
            var result = await mediator.Send(command, cancellationToken);
            return Accepted(new Envelope<bool>(result.StartsWith("Error") ? false : true ,result));
        }
    }
}
