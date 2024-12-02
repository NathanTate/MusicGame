export interface TokenDto {
  accessToken: TokenBase;
  refreshToken: TokenBase;
}

export interface TokenBase {
  token: string;
  expiresAt: Date;
}