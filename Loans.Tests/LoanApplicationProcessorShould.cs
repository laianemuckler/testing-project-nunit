using Loans.Domain.Applications;
using Moq;
using NUnit.Framework;
using System;
using Moq.Protected;

namespace Loans.Tests
{
    public class LoanApplicationProcessorShould
    {
        [Test]
        public void DeclineLowSalary()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  64_999);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();
            var mockCreditScorer = new Mock<ICreditScorer>();

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object, mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.False);
        }

        [Test]
        public void Accept()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            // need to configure the moq objects to return correct values 

            // attribute

            var mockIdentityVerifier = new Mock<IIdentityVerifier>(MockBehavior.Strict);

            mockIdentityVerifier.Setup(x => x.Initialize());


            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                        25,
                                                        "133 Pluralsight Drive, Draper, Utah"))
                                             .Returns(true);

            // property - mock property to return a specific value

            var mockCreditScorer = new Mock<ICreditScorer>();

            mockCreditScorer.SetupAllProperties(); // need to be before all setup to all properties

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);
            //mockCreditScorer.SetupProperty(x => x.Count); // tracking a change in a property

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            mockCreditScorer.VerifyGet(x => x.ScoreResult.ScoreValue.Score);
            mockCreditScorer.VerifySet(x => x.Count = It.IsAny<int>());

            Assert.That(application.GetIsAccepted(), Is.True);
            Assert.That(mockCreditScorer.Object.Count, Is.EqualTo(1));
        }

        [Test]
        public void InitializeIdentityVerifier()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();


            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                        25,
                                                        "133 Pluralsight Drive, Draper, Utah"))
                                             .Returns(true);

            var mockCreditScorer = new Mock<ICreditScorer>();

            mockCreditScorer.SetupAllProperties();

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            mockIdentityVerifier.Verify(x => x.Initialize());

            // explicit verification to validate method
            mockIdentityVerifier.Verify(x => x.Validate(It.IsAny<string>(),
                                                        It.IsAny<int>(),
                                                        It.IsAny<string>()));

            // to check if all the methods and properties were call
            mockIdentityVerifier.VerifyNoOtherCalls();
        }

        [Test]
        public void CalculateScore()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();


            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                        25,
                                                        "133 Pluralsight Drive, Draper, Utah"))
                                             .Returns(true);

            var mockCreditScorer = new Mock<ICreditScorer>();

            mockCreditScorer.SetupAllProperties();

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            mockCreditScorer.Verify(x => x.CalculateScore("Sarah", "133 Pluralsight Drive, Draper, Utah"), Times.Once);

            // tests if the method is call no meter whats values
            //mockCreditScorer.Verify(x => x.CalculateScore(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public void AcceptUsingPartialMock()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IdentityVerifierServiceGateway>();

            mockIdentityVerifier.Protected().Setup<bool>("CallService", "Sarah",
                                                          25,
                                                          "133 Pluralsight Drive, Draper, Utah")
                                .Returns(true);

            var expectedTime = new DateTime(2000, 1, 1);

            // protected members 
            mockIdentityVerifier.Protected().Setup<DateTime>("GetCurrentTime")
                                .Returns(expectedTime);

            var mockCreditScorer = new Mock<ICreditScorer>();
            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);


            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.True);
            Assert.That(mockIdentityVerifier.Object.LastCheckTime,
                        Is.EqualTo(expectedTime));

        }
    }
}
