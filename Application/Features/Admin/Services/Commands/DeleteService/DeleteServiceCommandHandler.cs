using Domain.Entities;
using Domain.Repositories;
using ErrorOr;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Admin.Services.Commands.DeleteService
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, ErrorOr<Success>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteServiceCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ErrorOr<Success>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var serviceRepo = _unitOfWork.Repository<Service, int>();
            var bookingItemRepo = _unitOfWork.Repository<BookingItem, int>();

            var service = await serviceRepo.GetByIdAsync(request.ServiceId);
            if (service == null)
                return Error.NotFound("service.not.found", "الخدمة دي مش موجودة.");

            var associatedBookingItems = await bookingItemRepo.FindAsync(bi => bi.ServiceId == request.ServiceId);
            if (associatedBookingItems.Any())
            {
                return Error.Conflict("service.has.bookings", "مينفعش تمسح الخدمة عشان مرتبطة بحجوزات قبل كده. ممكن تعطلها بدل ما تمسحها.");
            }

            serviceRepo.Delete(service);
            await _unitOfWork.CompleteAsync();

            return Result.Success;
        }
    }
}
