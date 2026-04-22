import { useContext } from "react";
import { AuthContext } from "./AuthContext"; // Ajuste o path se necessário

export const useAuth = () => {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error("useAuth deve ser usado dentro de um AuthProvider");
  }
  
  return context;
};