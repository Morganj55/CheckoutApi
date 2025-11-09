using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using PaymentGateway.Api.Clients;
using PaymentGateway.Api.Domain;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Services;
using PaymentGateway.Api.Tests.ModelValidation;
using PaymentGateway.Api.Tests.Stubs;
using PaymentGateway.Api.Utility;

namespace PaymentGateway.Api.Tests.Services
{
    public class PaymentRepositoryTests
    {
        private PaymentRequestResponse MakeValid()
        {
            return new PaymentRequestResponse
            {
                Id = Guid.NewGuid(),
                CardNumberLastFour = "1234",
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                Amount = 1000,
                Currency = "USD",
                Status = PaymentStatus.Authorized
            };
        }

        private static PaymentRequestResponse NewPayment(Guid? id = null, PaymentStatus status = PaymentStatus.Pending)
        {
            return new PaymentRequestResponse
            {
                Id = id ?? Guid.NewGuid(),
                Amount = 1234,
                Currency = "GBP",
                CardNumberLastFour = "4242",
                ExpiryMonth = 12,
                ExpiryYear = DateTime.Now.Year + 1,
                Status = status
            };
        }

        [Fact]
        public void Add_NewPayment_ReturnsSuccess_AndIncrementsCount()
        {
            var repo = new PaymentsRepository();
            var p = NewPayment();

            var before = repo.TotalPaymentCount;
            var result = repo.Add(p);

            Assert.True(result.IsSuccess);
            Assert.Equal(before + 1, repo.TotalPaymentCount);
        }

        [Fact]
        public void Add_DuplicateId_ReturnsFailure_AndDoesNotIncrementCount()
        {
            var repo = new PaymentsRepository();
            var id = Guid.NewGuid();
            var p1 = NewPayment(id);
            var p2 = NewPayment(id);

            var first = repo.Add(p1);
            Assert.True(first.IsSuccess);

            var countBefore = repo.TotalPaymentCount;
            var dup = repo.Add(p2);

            Assert.True(dup.IsFailure);
            Assert.NotNull(dup.Error);
            Assert.Equal(ErrorKind.Unexpected, dup.Error!.Kind);
            Assert.Equal(countBefore, repo.TotalPaymentCount);
        }

        [Fact]
        public async Task GetAsync_Existing_ReturnsSuccess_WithSameData()
        {
            var repo = new PaymentsRepository();
            var p = NewPayment();
            repo.Add(p);

            var got = await repo.GetAsync(p.Id);

            Assert.True(got.IsSuccess);
            Assert.NotNull(got.Data);
            Assert.Equal(p.Id, got.Data!.Id);
            Assert.Equal("4242", got.Data.CardNumberLastFour);
            Assert.Equal(PaymentStatus.Pending, got.Data.Status);
        }

        [Fact]
        public async Task GetAsync_Missing_ReturnsNotFound()
        {
            var repo = new PaymentsRepository();

            var got = await repo.GetAsync(Guid.NewGuid());

            Assert.True(got.IsFailure);
            Assert.NotNull(got.Error);
            Assert.Equal(ErrorKind.NotFound, got.Error!.Kind);
            Assert.Equal(HttpStatusCode.NotFound, got.Error.Code);
        }

        [Fact]
        public void Update_Existing_ChangesStatus_AndReturnsSuccess()
        {
            var repo = new PaymentsRepository();
            var original = NewPayment(status: PaymentStatus.Pending);
            var add = repo.Add(original);
            Assert.True(add.IsSuccess);

            var result = repo.UpdatePaymentStatus(original.Id, PaymentStatus.Authorized);
            Assert.True(result.IsSuccess);

            var fetched = repo.GetAsync(original.Id).GetAwaiter().GetResult();
            Assert.True(fetched.IsSuccess);
            Assert.Equal(PaymentStatus.Authorized, fetched.Data!.Status);
        }

        [Fact]
        public void Update_Missing_ReturnsNotFound()
        {
            var repo = new PaymentsRepository();

            var result = repo.UpdatePaymentStatus(Guid.NewGuid(), PaymentStatus.Authorized);

            Assert.True(result.IsFailure);
            Assert.NotNull(result.Error);
            Assert.Equal(ErrorKind.NotFound, result.Error!.Kind);
            Assert.Equal(HttpStatusCode.NotFound, result.Error.Code);
        }

        [Fact]
        public async Task Repository_IsThreadSafe_ForConcurrentAdds()
        {
            var repo = new PaymentsRepository();
            var ids = Enumerable.Range(0, 300).Select(_ => Guid.NewGuid()).ToArray();

            await Task.WhenAll(ids.Select(id => Task.Run(() =>
            {
                var p = NewPayment(id);
                repo.Add(p);
            })));

            Assert.Equal(ids.Length, repo.TotalPaymentCount);

            var gets = await Task.WhenAll(ids.Select(id => repo.GetAsync(id)));
            foreach (var r in gets)
            {
                Assert.True(r.IsSuccess);
                Assert.NotNull(r.Data);
            }
        }
    }
}
