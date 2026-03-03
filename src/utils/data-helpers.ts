import categoriesData from '@/data/categories.json';
import servicesData from '@/data/services.json';

export interface Category {
  id: string;
  name: string;
  slug: string;
  description: string;
  icon: string;
  sortOrder: number;
}

export interface Service {
  id: string;
  name: string;
  slug: string;
  description: string;
  longDescription: string;
  websiteUrl: string;
  affiliateUrl?: string;
  rating: number;
  isFeatured: boolean;
  isPopular: boolean;
  pros: string[];
  cons: string[];
  categories: string[];
}

const categories: Category[] = categoriesData as Category[];
const services: Service[] = servicesData as Service[];

export function getCategories(): Category[] {
  return [...categories].sort((a, b) => a.sortOrder - b.sortOrder);
}

export function getCategoryBySlug(slug: string): Category | undefined {
  return categories.find((c) => c.slug === slug);
}

export function getServicesByCategory(categorySlug: string): Service[] {
  return services
    .filter((s) => s.categories.includes(categorySlug))
    .sort((a, b) => b.rating - a.rating);
}

export function getServiceBySlug(slug: string): Service | undefined {
  return services.find((s) => s.slug === slug);
}

export function getFeaturedServices(count: number = 4): Service[] {
  return services
    .filter((s) => s.isFeatured)
    .sort((a, b) => b.rating - a.rating)
    .slice(0, count);
}

export function getAllServices(): Service[] {
  return [...services].sort((a, b) => b.rating - a.rating);
}

export function getRelatedServices(serviceId: string, categorySlug: string, count: number = 3): Service[] {
  return services
    .filter((s) => s.id !== serviceId && s.categories.includes(categorySlug))
    .sort((a, b) => b.rating - a.rating)
    .slice(0, count);
}

export function getPrimaryCategory(service: Service): Category | undefined {
  return categories.find((c) => c.slug === service.categories[0]);
}

export function getCategoryServiceCount(categorySlug: string): number {
  return services.filter((s) => s.categories.includes(categorySlug)).length;
}
