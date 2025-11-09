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
using PaymentGateway.Api.Tests.TestDataBuilders;

namespace PaymentGateway.Api.Tests.Services
{
    public class PaymentRepositoryTests
    {
        [Fact]
        public async Task Add_NewPayment_ReturnsSuccess_AndIncrementsCount()
        {
            var repo = new PaymentsRepository();
            var p = PaymentRequestResponseBuilder.Build(new Guid());

            var before = repo.TotalPaymentCount;
            var result = await repo.Add(p);

            Assert.True(result.IsSuccess);
            Assert.Equal(before + 1, repo.TotalPaymentCount);
        }

        [Fact]
        public async Task Add_DuplicateId_ReturnsFailure_AndDoesNotIncrementCount()
        {
            var repo = new PaymentsRepository();
            var id = Guid.NewGuid();
            var p1 = PaymentRequestResponseBuilder.Build(id);
            var p2 = PaymentRequestResponseBuilder.Build(id);

            var first = await repo.Add(p1);
            Assert.True(first.IsSuccess);

            var countBefore = repo.TotalPaymentCount;
            var dup = await repo.Add(p2);

            Assert.True(dup.IsFailure);
            Assert.NotNull(dup.Error);
            Assert.Equal(ErrorKind.Unexpected, dup.Error!.Kind);
            Assert.Equal(countBefore, repo.TotalPaymentCount);
        }

        [Fact]
        public async Task GetAsync_Existing_ReturnsSuccess_WithSameData()
        {
            var repo = new PaymentsRepository();
            var p = PaymentRequestResponseBuilder.Build(new Guid());
            await repo.Add(p);

            var got = await repo.GetAsync(p.Id);

            Assert.True(got.IsSuccess);
            Assert.NotNull(got.Data);
            Assert.Equal(p.Id, got.Data!.Id);
            Assert.Equal(p.CardNumberLastFour, got.Data.CardNumberLastFour);
            Assert.Equal(p.Status, got.Data.Status);
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
        public async Task Update_Existing_ChangesStatus_AndReturnsSuccess()
        {
            var repo = new PaymentsRepository();
            var original = PaymentRequestResponseBuilder.Build (new Guid(), PaymentStatus.Pending);
            var add = await repo.Add(original);
            Assert.True(add.IsSuccess);

            var result = await repo.UpdatePaymentStatus(original.Id, PaymentStatus.Authorized);
            Assert.True(result.IsSuccess);

            var fetched = repo.GetAsync(original.Id).GetAwaiter().GetResult();
            Assert.True(fetched.IsSuccess);
            Assert.Equal(PaymentStatus.Authorized, fetched.Data!.Status);
        }

        [Fact]
        public async Task Update_Missing_ReturnsNotFound()
        {
            var repo = new PaymentsRepository();

            var result = await repo.UpdatePaymentStatus(Guid.NewGuid(), PaymentStatus.Authorized);

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
                var p = PaymentRequestResponseBuilder.Build(id);
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
