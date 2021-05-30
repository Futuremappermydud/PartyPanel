export type Song =  {
  id: string,
  interpret: string,
  title: string,
  subtitle?: string,
  mapper?: string,
  difficulties: [string?, string?, string?, string?, string?]
};