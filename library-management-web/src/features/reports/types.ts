export interface OverdueLoan {
  loanId: string;
  memberId: string;
  memberName: string;
  bookId: string;
  bookTitle: string;
  branchId: string;
  branchName: string;
  borrowedAtUtc: string;
  dueDateUtc: string;
  daysOverdue: number;
}

export interface MostBorrowedBook {
  bookId: string;
  title: string;
  author: string;
  borrowCount: number;
}

export interface BranchInventorySummary {
  branchId: string;
  branchName: string;
  totalTitles: number;
  totalCopies: number;
  availableCopies: number;
  utilizationPercentage: number;
}

export interface MemberActivity {
  memberId: string;
  membershipNumber: string;
  memberName: string;
  activeLoanCount: number;
  totalLoanCount: number;
  overdueLoanCount: number;
  activeReservationCount: number;
}

export interface ReservationQueueSummary {
  bookId: string;
  bookTitle: string;
  branchId: string;
  branchName: string;
  pendingCount: number;
  hasReadyCopy: boolean;
  oldestPendingSinceUtc: string | null;
}
