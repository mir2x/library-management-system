export type MembershipStatus = 'Active' | 'Suspended' | 'Deactivated';

export interface Member {
  id: string;
  membershipNumber: string;
  fullName: string;
  email: string;
  phone: string | null;
  address: string | null;
  homeBranchId: string;
  homeBranchName: string;
  status: MembershipStatus;
  joinDateUtc: string;
}

export interface CreateMemberRequest {
  fullName: string;
  email: string;
  phone: string | null;
  address: string | null;
  homeBranchId: string;
}

export interface UpdateMemberRequest {
  fullName: string | null;
  email: string | null;
  phone: string | null;
  address: string | null;
  homeBranchId: string | null;
}
