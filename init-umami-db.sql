-- Create umami database and user for Umami analytics
-- This runs on first postgres init only

SELECT 'CREATE DATABASE umami' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'umami')\gexec
DO $$ BEGIN
  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'umami') THEN
    CREATE ROLE umami WITH LOGIN PASSWORD 'umami';
  END IF;
END $$;
GRANT ALL PRIVILEGES ON DATABASE umami TO umami;
