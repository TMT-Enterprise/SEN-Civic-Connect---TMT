
const { Pool } = require("pg");
const fs = require("node:fs/promises");
const path = require("node:path");

const connectionString = process.env.CIVICCONNECT_DATABASE_URL;

if (
  !connectionString &&
  !process.env.CIVICCONNECT_DB_PASSWORD
) {
  console.error(
    "Migration failed: set CIVICCONNECT_DATABASE_URL or CIVICCONNECT_DB_PASSWORD."
  );
  process.exit(1);
}

const pool = connectionString
  ? new Pool({ connectionString })
  : new Pool({
      host: process.env.CIVICCONNECT_DB_HOST || "localhost",
      port: Number(process.env.CIVICCONNECT_DB_PORT || 5432),
      database: process.env.CIVICCONNECT_DB_NAME || "TMT_civicconnect",
      user: process.env.CIVICCONNECT_DB_USER || "postgres",
      password: process.env.CIVICCONNECT_DB_PASSWORD,
    });

const migrationsDirectory = path.resolve(
  __dirname,
  "../migrations"
);

async function runMigrations() {
  const client = await pool.connect();
  let lockAcquired = false;

  try {
    await client.query(
      "SELECT pg_advisory_lock($1)",
      [3812026]
    );
    lockAcquired = true;

    await client.query(`
      CREATE TABLE IF NOT EXISTS public.schema_migrations (
        migration_name TEXT PRIMARY KEY,
        applied_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
      );
    `);

    const files = (await fs.readdir(migrationsDirectory))
      .filter((file) => /^\d+_.+\.sql$/.test(file))
      .sort();

    if (files.length === 0) {
      console.log("No migration files found.");
      return;
    }

    for (const file of files) {
      const existing = await client.query(
        `SELECT 1
         FROM public.schema_migrations
         WHERE migration_name = $1`,
        [file]
      );

      if (existing.rowCount > 0) {
        console.log(`SKIPPED: ${file} already applied.`);
        continue;
      }

      const sql = await fs.readFile(
        path.join(migrationsDirectory, file),
        "utf8"
      );

      console.log(`RUNNING: ${file}`);

      await client.query("BEGIN");

      try {
        await client.query(sql);

        await client.query(
          `INSERT INTO public.schema_migrations (migration_name)
           VALUES ($1)`,
          [file]
        );

        await client.query("COMMIT");
        console.log(`SUCCESS: ${file}`);
      } catch (error) {
        await client.query("ROLLBACK");
        console.error(`FAILED: ${file}`);
        throw error;
      }
    }

    console.log("All pending migrations completed.");
  } finally {
    if (lockAcquired) {
      try {
        await client.query(
          "SELECT pg_advisory_unlock($1)",
          [3812026]
        );
      } finally {
        client.release();
      }
    } else {
      client.release();
    }
  }
}

runMigrations()
  .catch((error) => {
    console.error("Migration error:", error.message);
    process.exitCode = 1;
  })
  .finally(async () => {
    await pool.end();
  });
