import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import {
  LayoutDashboard, Briefcase, Key, FileText, Play, LogOut,
} from 'lucide-react';

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/workflows', label: 'Workflows', icon: Play },
  { to: '/providers', label: 'AI Providers', icon: Key },
  { to: '/documents', label: 'Documents', icon: FileText },
];

export function Navbar() {
  const { user, logout } = useAuth();
  const location = useLocation();

  return (
    <nav className="flex h-14 items-center border-b bg-white px-4">
      <Link to="/dashboard" className="flex items-center gap-2 font-semibold">
        <Briefcase className="h-5 w-5" />
        <span>Browser Agent</span>
      </Link>

      <div className="ml-8 flex items-center gap-1">
        {navItems.map((item) => {
          const Icon = item.icon;
          const active = location.pathname.startsWith(item.to);
          return (
            <Link
              key={item.to}
              to={item.to}
              className={`flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm transition-colors ${
                active ? 'bg-primary text-primary-foreground' : 'hover:bg-muted'
              }`}
            >
              <Icon className="h-4 w-4" />
              {item.label}
            </Link>
          );
        })}
      </div>

      <div className="ml-auto flex items-center gap-3">
        <span className="text-sm text-muted-foreground">
          {user?.firstName ?? user?.email}
        </span>
        <button
          onClick={logout}
          className="flex items-center gap-1 rounded-md px-2 py-1 text-sm text-muted-foreground hover:bg-muted"
        >
          <LogOut className="h-4 w-4" />
          Logout
        </button>
      </div>
    </nav>
  );
}
