/*export interface NavItem {
  label: string;
  route?: string; // Optional: If it has children, it might not need a base route
  iconSvg: string; // The raw SVG string
  children?: NavItem[]; // Optional: For Level 1 (L1) sub-menus
}*/
export interface NavItem {
  label: string;
  iconSvg: string;
  route?: string;
  badge?: number | null;
  children?: {
    label: string;
    route: string;
    badge?: number | null;
  }[];
  permissions?: string[];
}
