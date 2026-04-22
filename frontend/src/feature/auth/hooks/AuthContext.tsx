import { createContext, useState, useEffect, useCallback, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { AuthService } from "../services/AuthService";
import { tokenService } from "@/core/http/TokenService";
import { AlertService } from "@/core/alert/AlertService";
import type { LoginRequestDTO, UserDTO, RegisterRequestDTO } from "../types";

const USE_AUTH_MOCK = false;

interface AuthContextData {
  user: UserDTO | null;
  loading: boolean;
  error: string | null;
  isMockMode: boolean;
  login: (credentials: LoginRequestDTO) => Promise<void>;
  register: (data: RegisterRequestDTO) => Promise<boolean>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextData | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<UserDTO | null>(null);
  const [loading, setLoading] = useState<boolean>(true); // Começa como true para evitar piscar telas deslogadas
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  // Roda apenas UMA VEZ na raiz da aplicação
  useEffect(() => {
    let isActive = true;

    const bootstrapSession = async () => {
      try {
        if (!tokenService.getToken()) {
          setLoading(false);
          return;
        }

        const userData = await AuthService.getMe(USE_AUTH_MOCK);
        if (isActive) setUser(userData);
      } catch {
        tokenService.removeToken();
        if (isActive) setUser(null);
      } finally {
        if (isActive) setLoading(false);
      }
    };

    void bootstrapSession();

    return () => { isActive = false; };
  }, []);

  const login = async (credentials: LoginRequestDTO) => {
    setLoading(true);
    setError(null);
    try {
      const response = await AuthService.login(credentials, USE_AUTH_MOCK);
      tokenService.saveToken(response.access_token);

      const userData = await AuthService.getMe(USE_AUTH_MOCK);
      setUser(userData);

      navigate("/dashboard", { replace: true });
    } catch (err) {
      const message = err instanceof Error ? err.message : "Falha na autenticação.";
      setError(message);
      AlertService.error("Erro", message);
    } finally {
      setLoading(false);
    }
  };

  const register = async (data: RegisterRequestDTO): Promise<boolean> => {
    setLoading(true);
    setError(null);
    try {
      await AuthService.register(data, USE_AUTH_MOCK);
      AlertService.success("Cadastro realizado com sucesso!");
      navigate("/login", { replace: true });
      return true;
    } catch (err) {
      const message = err instanceof Error ? err.message : "Falha ao realizar o cadastro.";
      setError(message);
      return false;
    } finally {
      setLoading(false);
    }
  };

  const logout = useCallback(async () => {
    try {
      await AuthService.logout(USE_AUTH_MOCK);
    } finally {
      tokenService.removeToken();
      setUser(null);
      setError(null);
      navigate("/login", { replace: true });
    }
  }, [navigate]);

  return (
    <AuthContext.Provider value={{ user, loading, error, isMockMode: USE_AUTH_MOCK, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
};