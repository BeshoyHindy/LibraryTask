using Lending.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lending.Data;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(loan => loan.Id);

        builder.HasOne<Borrower>().WithMany().HasForeignKey(loan => loan.BorrowerId);

        builder.HasIndex(loan => loan.BookId);
        builder.HasIndex(loan => loan.BorrowerId);
        builder.HasIndex(loan => loan.BorrowedOn);

        // One open loan per book holds in the database as well as in the handler.
        builder.HasIndex(loan => loan.BookId, "IX_Loans_BookId_Open")
            .HasFilter(@"""ReturnedOn"" IS NULL")
            .IsUnique();
    }
}
