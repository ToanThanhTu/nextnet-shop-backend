--
-- PostgreSQL database dump
--

\restrict GrKaIeKbB1mbAD8XFOJDcIk7vl0WJ3vcfu4wiU58RIDptRAhi9sFWAwbJD6uElo

-- Dumped from database version 17.9
-- Dumped by pg_dump version 17.9

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Data for Name: categories; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.categories (id, title, description, image) VALUES (1, 'Home', 'Home Products', NULL);
INSERT INTO public.categories (id, title, description, image) VALUES (2, 'Electronics', 'Electronics Products', NULL);
INSERT INTO public.categories (id, title, description, image) VALUES (3, 'Fitness', 'Fitness Products', NULL);


--
-- Data for Name: subcategories; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (1, 'Bedding', 'Bedding Products', 1, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (2, 'Living', 'Living Products', 1, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (3, 'Kitchen', 'Kitchen Products', 1, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (4, 'Garden', 'Garden Products', 1, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (5, 'Appliances', 'Appliances Products', 2, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (6, 'Laptops', 'Laptops Products', 2, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (7, 'Mobile Phones', 'Mobile Phones Products', 2, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (8, 'Cardio', 'Cardio Products', 3, NULL);
INSERT INTO public.subcategories (id, title, description, category_id, image) VALUES (9, 'Weights', 'Weights Products', 3, NULL);


--
-- Data for Name: products; Type: TABLE DATA; Schema: public; Owner: -
--

INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (0, 'Bedframe Super X v3.0', 'Bedframe Super X v3.1', 599.99, 15, 0, 5, 1, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (1, 'Sofa Enhanced V2', 'Sofa Enhanced V3', 399.99, 6, 10, 4, 2, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (2, 'Coffee Machine Advanced D5', 'Coffee Machine Advanced D6', 199.99, 7, 35, 10, 3, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (3, 'Hose Hose Hose GB32', 'Hose Hose Hose GB33', 39.99, 56, 0, 45, 4, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (4, 'Jumping Rope Long', 'Jumping Rope Long', 9.99, 15, 0, 12, 8, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (5, 'Heavy Dumbbell 2kg', 'Heavy Dumbbell 2kg', 5.99, 16, 0, 7, 9, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (6, 'Refridgerator Cold and Colder', 'Refridgerator Cold and Colder', 549.99, 67, 63, 35, 5, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (7, 'uPhone 23 Pro Mex', 'uPhone 23 Pro Mex', 799.99, 22, 0, 23, 7, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (8, 'Heater Hot and Hotter 77', 'Heater Hot and Hotter 78', 159.99, 11, 20, 14, 5, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (9, 'Allianceware Pro Gaming 17', 'Allianceware Pro Gaming 18', 1999.99, 64, 0, 18, 6, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (10, 'Dishwasher BrokenPlates 7000', 'Dishwasher BrokenPlates 7001', 529.99, 8, 54, 22, 3, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (11, 'Kettle BoilingHot 99', 'Kettle BoilingHot 100', 19.99, 47, 0, 75, 5, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (12, 'Dull Laptop Toyish 4', 'Dull Laptop Toyish 5', 24.99, 28, 0, 31, 6, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (13, 'Nekia Fakeish 3210', 'Nekia Fakeish 3211', 23.99, 34, 0, 39, 7, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (14, 'Microwave Oven 3000', 'Microwave Oven 3001', 18.99, 2, 0, 21, 3, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (15, 'PANda StirFry Master 2', 'PANda StirFry Master 3', 15.99, 1, 0, 10, 3, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (16, 'POTluck Souping 55', 'POTluck Souping 56', 22.99, 26, 0, 17, 3, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (17, 'Razer Barracuda X Chroma', 'Razer Barracuda X Chroma', 329.99, 7, 0, 9, 5, NULL);
INSERT INTO public.products (id, title, description, price, stock, sale, sold, subcategory_id, image) VALUES (18, 'Octopas Rift 24', 'Octopas Rift 25', 129.99, 6, 25, 11, 5, NULL);


--
-- Name: categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.categories_id_seq', 3, true);


--
-- Name: products_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.products_id_seq', 18, true);


--
-- Name: subcategories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: -
--

SELECT pg_catalog.setval('public.subcategories_id_seq', 9, true);


--
-- PostgreSQL database dump complete
--

\unrestrict GrKaIeKbB1mbAD8XFOJDcIk7vl0WJ3vcfu4wiU58RIDptRAhi9sFWAwbJD6uElo

