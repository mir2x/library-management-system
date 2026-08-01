export type LoanStatus = 'Active' | 'Returned';

export interface Loan {
  id: string;
  memberId: string;
  memberName: string;
  bookId: string;
  bookTitle: string;
  branchId: string;
  branchName: string;
  borrowedAtUtc: string;
  dueDateUtc: string;
  returnedAtUtc: string | null;
  status: LoanStatus;
  isOverdue: boolean;
}

export interface BorrowBookRequest {
  memberId: string;
  bookId: string;
  branchId: string;
}
