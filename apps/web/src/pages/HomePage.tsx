export default function HomePage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center p-8">
      <header className="mb-12 text-center">
        <h1 className="text-4xl font-bold tracking-tight">Browser Agent Framework</h1>
        <p className="mt-4 text-lg text-muted-foreground">
          An open-source platform for AI-powered browser automation agents.
        </p>
      </header>

      <main className="flex max-w-2xl flex-col gap-6 text-center">
        <div className="rounded-lg border bg-card p-6 text-card-foreground shadow-sm">
          <h2 className="text-xl font-semibold">Getting Started</h2>
          <p className="mt-2 text-muted-foreground">
            Configure your AI provider, upload your resume, and start automating job applications.
          </p>
        </div>

        <div className="grid gap-4 sm:grid-cols-3">
          <div className="rounded-lg border bg-card p-4 text-card-foreground shadow-sm">
            <h3 className="font-semibold">Connect AI</h3>
            <p className="mt-1 text-sm text-muted-foreground">
              Bring your own API keys for DeepSeek or Ollama.
            </p>
          </div>
          <div className="rounded-lg border bg-card p-4 text-card-foreground shadow-sm">
            <h3 className="font-semibold">Upload Resume</h3>
            <p className="mt-1 text-sm text-muted-foreground">
              Store multiple resumes for different roles.
            </p>
          </div>
          <div className="rounded-lg border bg-card p-4 text-card-foreground shadow-sm">
            <h3 className="font-semibold">Run Workflow</h3>
            <p className="mt-1 text-sm text-muted-foreground">
              Paste a job URL and let the browser agent handle the rest.
            </p>
          </div>
        </div>
      </main>

      <footer className="mt-auto pt-12 text-sm text-muted-foreground">
        Phase 1 &mdash; Project Foundation
      </footer>
    </div>
  );
}
