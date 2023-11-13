using AutoMapper;
using EventStore.Client;
using MediatR;
using Publisher.Contract.Requests.BookCreated;
using Shared.Kernel.Absract;
using Shared.Kernel.Abstracts;
using Shared.Kernel.AutoMapper;

namespace Publisher.Service.Commands.BookCreated
{
    #region Command
    public class BookCreatedCommand : IBaseCommand<BookCreatedRequest, BookCreatedCommand>, IRequest<string>, IEventData
    {
        #region Properties
        public string BookName { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public DateTime CreatedDateTime { get; set; }
        #endregion

        #region Constructor
        public BookCreatedCommand(string bookName, string author, string publisher, DateTime createdDateTime)
        {
            BookName = bookName;
            Author = author;
            Publisher = publisher;
            CreatedDateTime = createdDateTime;
        }
        #endregion

        #region Methods
        public static BookCreatedCommand FromRequest(BookCreatedRequest request) => WithProfile<BookCreatedCommandProfile>.Map<BookCreatedCommand>(request);

        public class BookCreatedCommandHandler : IRequestHandler<BookCreatedCommand, string>
        {
            private readonly IEventStoreWriteRepository<BookCreatedCommand> writeRepository;
            public BookCreatedCommandHandler(IEventStoreWriteRepository<BookCreatedCommand> _writeRepository) => writeRepository = _writeRepository;
            public async Task<string> Handle(BookCreatedCommand request, CancellationToken cancellationToken)
            {
                var response = await writeRepository.AppendToStream(request, StreamState.Any, cancellationToken: cancellationToken);
                return $"{response.CommitPosition}";
            }
        }
        #endregion
    }
    #endregion

    #region AutoMapper Profile
    public class BookCreatedCommandProfile : Profile
    {
        public BookCreatedCommandProfile()
        {
            CreateMap<BookCreatedRequest, BookCreatedCommand>()
                 .ConstructUsing(x => new BookCreatedCommand
                  (
                     x.BookName,
                     x.Author,
                     x.Publisher,
                     x.CreatedDateTime
                 ));
        }
    }
    #endregion
}
