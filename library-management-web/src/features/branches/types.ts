export interface Branch {
  id: string;
  name: string;
  address: string;
  contactNumber: string | null;
  email: string | null;
  isActive: boolean;
}

export interface CreateBranchRequest {
  name: string;
  address: string;
  contactNumber: string | null;
  email: string | null;
}

export interface UpdateBranchRequest {
  name: string | null;
  address: string | null;
  contactNumber: string | null;
  email: string | null;
}
