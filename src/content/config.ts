import { defineCollection, z } from 'astro:content';

const articles = defineCollection({
  type: 'content',
  schema: z.object({
    title: z.string(),
    metaDescription: z.string(),
    categorySlug: z.string().optional(),
    updatedAt: z.string(),
  }),
});

export const collections = { articles };
