import { TokenWrapper } from "./tokenDto";

export interface AuthData {
  userId: string;
  email: string;
  username: string;
  roles: string[];
  profilePhotoUrl?: string;
  tokens: TokenWrapper;
}